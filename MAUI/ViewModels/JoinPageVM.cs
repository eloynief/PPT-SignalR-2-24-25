using ENT;
using FUNC;
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
    public class JoinPageVM: MiNotifyChanged
    {

        #region atributos
        private readonly HubConnection connection;
        //private List<Grupo> grupos;

        private Grupo choosenGroup;

        //si se ha pulsado el boton
        private bool botonJoinPulsado;
        private bool botonCancelarPulsado;
        //jugador y grupo
        private string playerName;
        private string selectedGroup;
        #endregion

        #region properties
        /**
        public List<Grupo> Grupos
        {
            get { return grupos; }
            set { 
                grupos = value;
                OnPropertyChanged(nameof(Grupos));
            }
        }
        */

        public Grupo ChoosenGroup
        {
            get { return choosenGroup; }
            set
            {
                choosenGroup = value;
                OnPropertyChanged(nameof(ChoosenGroup));
            }
        }


        public bool BotonJoinPulsado
        {
            get { return botonJoinPulsado; }
            set {
                botonJoinPulsado = value;
                OnPropertyChanged(nameof(BotonJoinPulsado));
            }
        }
        public bool BotonCancelarPulsado
        {
            get { return botonCancelarPulsado; }
            set { 
                botonCancelarPulsado = value;
                OnPropertyChanged(nameof(BotonCancelarPulsado));
            }
        }

        public string PlayerName
        {
            get { return playerName; }
            set { 
                playerName = value;
                OnPropertyChanged(PlayerName);
            }
        }

        public string SelectedGroup {
            get { return selectedGroup; }
            set { 
                selectedGroup = value;
                OnPropertyChanged(SelectedGroup);
            }
        }
        #endregion

        #region commands
        public DelegateCommand JoinPulsadoCommand 
        { 
            get; 
            set;
        }
        public DelegateCommand CancelarPulsadoCommand
        {
            get; 
            set;
        }
        #endregion


        #region constructor
        public JoinPageVM()
        {
            //lista grupos
            //Grupos = new List<Grupo>();

            BotonJoinPulsado = false;
            BotonCancelarPulsado = false;

            //nos conectamos
            connection = new HubConnectionBuilder()
            .WithUrl(EnlaceJuego.enlace)
            .WithAutomaticReconnect()
            .Build();

            //botones
            JoinPulsadoCommand = new DelegateCommand(async () => await JoinGroupAsync(), () => !BotonJoinPulsado);
            CancelarPulsadoCommand = new DelegateCommand(async () => await CancelAsync(), () => !BotonCancelarPulsado);
            
            InitializeSignalRListeners();

            Conectarse();
        }
        #endregion


        #region funciones
        /// <summary>
        /// metodo para conectarse al servidor de signalR
        /// </summary>
        public async void Conectarse()
        {
            try
            {
                await connection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            if (connection.State == HubConnectionState.Connected)
            {
                Console.WriteLine(connection.State);
            }

        }

        public async Task JoinGroupAsync()
        {
            if (!string.IsNullOrWhiteSpace(PlayerName) && !string.IsNullOrWhiteSpace(SelectedGroup))
            {

                BotonJoinPulsado = true; // Deshabilitar botón de unirse

                JoinPulsadoCommand.RaiseCanExecuteChanged();
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "Nombre del jugador y grupo son requeridos", "OK");
            }

            try
            {
                await connection.InvokeAsync("JoinGroup", SelectedGroup, PlayerName);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error al unirse: {ex.Message}", "OK");
                BotonJoinPulsado = false; // Rehabilitar botón si falla
                JoinPulsadoCommand.RaiseCanExecuteChanged();
            }
        }


        public async Task CancelAsync()
        {
            BotonCancelarPulsado = true;
            CancelarPulsadoCommand.RaiseCanExecuteChanged();

            try
            {
                await connection.StopAsync();
                PlayerName = string.Empty;
                SelectedGroup = string.Empty;
                ChoosenGroup = null;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error al cancelar: {ex.Message}", "OK");
            }
            finally
            {
                BotonCancelarPulsado = false;
                CancelarPulsadoCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region funciones privadas
        /// <summary>
        /// metodo para inicializar el 
        /// </summary>
        private void InitializeSignalRListeners()
        {
            connection.On<int>("PlayerCountUpdated", (count) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (count == 1)
                    {
                        await App.Current.MainPage.DisplayAlert("Esperando", "Esperando a otro jugador...", "OK");
                    }
                });
            });

            connection.On<List<string>>("GameReady", (players) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await App.Current.MainPage.Navigation.PushAsync(new GamePage());
                });
            });

            connection.On<string>("Error", (message) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await App.Current.MainPage.DisplayAlert("Error", message, "OK");
                    BotonJoinPulsado = false;
                });
            });
        }
        #endregion

    }
}
