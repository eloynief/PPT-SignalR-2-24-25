using ENT;
using FUNC;
using MAUI.Notify;
using MAUI.ViewModels.Utilities;
using Microsoft.AspNetCore.SignalR.Client;

namespace MAUI.ViewModels
{
    public class GamePageVM: MiNotifyChanged
    {
        #region atributos
        private Jugador jugadorYo;
        private Jugador jugadorRival;
        private bool opcionElegida;
        private HubConnection connection;
        private string groupName;
        #endregion


        #region properties
        public Jugador JugadorYo
        {
            get => jugadorYo;
            set
            {
                jugadorYo = value;
                OnPropertyChanged(nameof(JugadorYo));
            }
        }

        public Jugador JugadorRival
        {
            get => jugadorRival;
            set
            {
                jugadorRival = value;
                OnPropertyChanged(nameof(JugadorRival));
            }
        }

        public bool OpcionElegida
        {
            get => opcionElegida;
            set
            {
                opcionElegida = value;
                OnPropertyChanged(nameof(OpcionElegida));
            }
        }
        #endregion

        #region commands
        public DelegateCommand ElegirPiedraCommand { get; }
        public DelegateCommand ElegirPapelCommand { get; }
        public DelegateCommand ElegirTijeraCommand { get; }
        #endregion




        #region constructors

        public GamePageVM()
        {
            // Inicializar la conexión (podría ser un singleton o servicio compartido)
            connection = new HubConnectionBuilder()
                .WithUrl(EnlaceJuego.enlace)
                .WithAutomaticReconnect()
                .Build();

            // Obtener datos del estado compartido (esto es un ejemplo, ajusta según tu implementación)
            groupName = App.CurrentGroupName; // Asumiendo una propiedad estática en App
            string playerName = App.CurrentPlayerName; // Asumiendo una propiedad estática en App

            JugadorYo = new Jugador(playerName ?? "Jugador 1", groupName ?? "Grupo Desconocido", 0, 0, "");
            JugadorRival = new Jugador("Esperando rival", groupName ?? "Grupo Desconocido", 0, 0, "");
            OpcionElegida = false;

            ElegirPiedraCommand = new DelegateCommand(async () => await ElegirOpcionAsync("Piedra"), () => !OpcionElegida);
            ElegirPapelCommand = new DelegateCommand(async () => await ElegirOpcionAsync("Papel"), () => !OpcionElegida);
            ElegirTijeraCommand = new DelegateCommand(async () => await ElegirOpcionAsync("Tijera"), () => !OpcionElegida);

            InitializeSignalRListeners();
            Task.Run(Conectarse);
        }
        #endregion



        #region funciones de llamada
        public async Task Conectarse()
        {
            if (connection.State != HubConnectionState.Connected)
            {
                try
                {
                    await connection.StartAsync();
                }
                catch (Exception ex)
                {
                    await App.Current.MainPage.DisplayAlert("Error", $"No se pudo conectar: {ex.Message}", "OK");
                }
            }
        }

        private async Task ElegirOpcionAsync(string opcion)
        {
            OpcionElegida = true;
            JugadorYo.Eleccion = opcion;

            try
            {
                await connection.InvokeAsync("ElegirOpcion", groupName, JugadorYo.Nombre, opcion);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error al enviar opción: {ex.Message}", "OK");
                OpcionElegida = false;
            }

            ElegirPiedraCommand.RaiseCanExecuteChanged();
            ElegirPapelCommand.RaiseCanExecuteChanged();
            ElegirTijeraCommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region funciones privadas
        private void InitializeSignalRListeners()
        {
            connection.On<string, string>("OpponentChose", (opponentName, opcion) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (opponentName != JugadorYo.Nombre)
                    {
                        JugadorRival.Nombre = opponentName;
                        JugadorRival.Eleccion = opcion;
                    }
                });
            });

            connection.On<string>("GameResult", (ganador) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    //var victoriaVm = new VictoriaPageVM(connection, groupName, ganador);
                    //await App.Current.MainPage.Navigation.PushAsync(new VictoriaPage(victoriaVm));
                });
            });

            connection.On<string>("Error", (message) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await App.Current.MainPage.DisplayAlert("Error", message, "OK");
                });
            });
        }
        #endregion



    }
}
