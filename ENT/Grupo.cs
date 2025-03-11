using System;
using System.Collections.Generic;

namespace ENT
{
    public class Grupo
    {
        #region atributos
        private string nombre;
        private List<Jugador> jugadores;
        #endregion

        #region properties
        public string Nombre
        {
            get { return nombre; }
            set { nombre = value; }
        }

        public List<Jugador> Jugadores
        {
            get { return jugadores; }
            set { jugadores = value; }
        }
        #endregion

        #region constructors
        /// <summary>
        /// constructor defecto
        /// </summary>
        public Grupo()
        {
            nombre = "";
            jugadores = new List<Jugador>();
        }

        /// <summary>
        /// constructor con params
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="jugadores"></param>
        public Grupo(string nombre, List<Jugador> jugadores)
        {
            this.nombre = nombre;
            //si el grupo no tiene jugadores
            this.jugadores = jugadores ?? new List<Jugador>();
        }
        #endregion
    }
}