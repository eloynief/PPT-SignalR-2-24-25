using Microsoft.AspNetCore.SignalR;

namespace ASP.Hubs
{
    public class GameHub:Hub
    {
        // Diccionario para rastrear jugadores por grupo
        private static readonly Dictionary<string, List<string>> _groupPlayers = new Dictionary<string, List<string>>();
        // Diccionario para rastrear las elecciones de los jugadores por grupo
        private static readonly Dictionary<string, Dictionary<string, string>> _groupChoices = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Permite a un jugador unirse a un grupo. Si ya hay 2 jugadores, inicia el juego.
        /// </summary>
        public async Task JoinGroup(string groupName, string playerName)
        {
            if (string.IsNullOrWhiteSpace(groupName) || string.IsNullOrWhiteSpace(playerName))
            {
                await Clients.Caller.SendAsync("Error", "El nombre del grupo y del jugador son requeridos.");
                return;
            }

            if (!_groupPlayers.ContainsKey(groupName))
            {
                _groupPlayers[groupName] = new List<string>();
            }

            var group = _groupPlayers[groupName];
            if (group.Count >= 2)
            {
                await Clients.Caller.SendAsync("Error", "El grupo ya está lleno.");
                return;
            }

            if (!group.Contains(playerName))
            {
                group.Add(playerName);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                // Notificar a todos en el grupo el conteo actualizado
                await Clients.Group(groupName).SendAsync("PlayerCountUpdated", group.Count);

                // Si hay 2 jugadores, notificar que el juego está listo
                if (group.Count == 2)
                {
                    await Clients.Group(groupName).SendAsync("GameReady", group);
                }
            }
        }

        /// <summary>
        /// Permite a un jugador salir de un grupo.
        /// </summary>
        public async Task LeaveGroup(string groupName, string playerName)
        {
            if (_groupPlayers.ContainsKey(groupName))
            {
                var group = _groupPlayers[groupName];
                if (group.Remove(playerName))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                    // Limpiar elecciones si existen
                    if (_groupChoices.ContainsKey(groupName))
                    {
                        _groupChoices[groupName].Remove(playerName);
                        if (_groupChoices[groupName].Count == 0)
                        {
                            _groupChoices.Remove(groupName);
                        }
                    }

                    // Notificar al grupo restante del conteo actualizado
                    await Clients.Group(groupName).SendAsync("PlayerCountUpdated", group.Count);

                    // Notificar al cliente que salió exitosamente
                    await Clients.Caller.SendAsync("LeftGroup", "Has salido del grupo.");
                }
            }
        }

        /// <summary>
        /// Registra la opción elegida por un jugador y verifica si ambos han elegido.
        /// </summary>
        public async Task ElegirOpcion(string groupName, string playerName, string opcion)
        {
            if (!_groupPlayers.ContainsKey(groupName) || !_groupPlayers[groupName].Contains(playerName))
            {
                await Clients.Caller.SendAsync("Error", "No estás en este grupo.");
                return;
            }

            if (!_groupChoices.ContainsKey(groupName))
            {
                _groupChoices[groupName] = new Dictionary<string, string>();
            }

            var choices = _groupChoices[groupName];
            choices[playerName] = opcion;

            // Notificar a todos en el grupo que alguien eligió
            await Clients.Group(groupName).SendAsync("OpponentChose", playerName, opcion);

            // Si ambos jugadores han elegido, determinar el ganador
            if (choices.Count == 2)
            {
                await VerGanador(groupName);
            }
        }

        /// <summary>
        /// Determina y notifica al grupo quién ganó basado en las elecciones.
        /// </summary>
        public async Task VerGanador(string groupName)
        {
            if (!_groupChoices.ContainsKey(groupName) || _groupChoices[groupName].Count != 2)
            {
                await Clients.Group(groupName).SendAsync("Error", "No hay suficientes elecciones para determinar un ganador.");
                return;
            }

            var choices = _groupChoices[groupName];
            var players = _groupPlayers[groupName];
            var player1 = players[0];
            var player2 = players[1];
            var choice1 = choices[player1];
            var choice2 = choices[player2];

            string winner = DetermineWinner(player1, choice1, player2, choice2);
            await Clients.Group(groupName).SendAsync("GameResult", winner);

            // Limpiar elecciones para permitir un nuevo juego si es necesario
            _groupChoices.Remove(groupName);
        }

        /// <summary>
        /// Lógica para determinar el ganador en Piedra, Papel, Tijeras.
        /// </summary>
        private string DetermineWinner(string player1, string choice1, string player2, string choice2)
        {
            if (choice1 == choice2) return "Empate";
            if ((choice1 == "Piedra" && choice2 == "Tijera") ||
                (choice1 == "Papel" && choice2 == "Piedra") ||
                (choice1 == "Tijera" && choice2 == "Papel"))
                return player1;
            return player2;
        }

        /// <summary>
        /// Maneja la desconexión de un jugador.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var group in _groupPlayers)
            {
                if (group.Value.Remove(Context.ConnectionId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, group.Key);
                    await Clients.Group(group.Key).SendAsync("PlayerCountUpdated", group.Value.Count);

                    if (_groupChoices.ContainsKey(group.Key))
                    {
                        _groupChoices[group.Key].Remove(Context.ConnectionId);
                        if (_groupChoices[group.Key].Count == 0)
                        {
                            _groupChoices.Remove(group.Key);
                        }
                    }
                    break;
                }
            }
            await base.OnDisconnectedAsync(exception);
        }


    }

}
