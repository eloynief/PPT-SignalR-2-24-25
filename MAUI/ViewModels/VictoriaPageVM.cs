using ENT;
using MAUI.Notify;
using MAUI.ViewModels.Utilities;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUI.ViewModels
{
    internal class VictoriaPageVM: MiNotifyChanged
    {
        #region atributos
        private Grupo grupo;
        private string ganador;
        private readonly HubConnection connection;
        private readonly string groupName;
        #endregion

        #region properties
        public Grupo Grupo
        {
            get => grupo;
            set
            {
                grupo = value;
                OnPropertyChanged(nameof(Grupo));
            }
        }

        public string Ganador
        {
            get => ganador;
            set
            {
                ganador = value;
                OnPropertyChanged(nameof(Ganador));
            }
        }
        #endregion

        #region commands
        public DelegateCommand IrseJuegoCommand { get; }
        #endregion

        #region constructor


        public VictoriaPageVM()
        {

        }


        /**
        public VictoriaPageVM(HubConnection connection, string groupName, string ganador)
        {
            this.connection = connection;
            this.groupName = groupName;

            Grupo = new Grupo(groupName, new List<Jugador>()); 
            Ganador = ganador;

            IrseJuegoCommand = new DelegateCommand(async () => await LeaveGameAsync());

            Task.Run(Conectarse);
        }
        */
        #endregion

        #region funciones asincronas
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
        #endregion

        #region funciones privadas
        private async Task LeaveGameAsync()
        {
            try
            {
                await connection.InvokeAsync("LeaveGroup", groupName, Grupo.Jugadores.FirstOrDefault()?.Nombre ?? "Unknown");
                await App.Current.MainPage.Navigation.PopToRootAsync();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error al salir: {ex.Message}", "OK");
            }
        }
        #endregion
    }
}
