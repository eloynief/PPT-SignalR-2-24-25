namespace ENT
{
    public class Jugador
    {
        #region atributos
        private string nombre;
        private string grupo;
        private int puntos;
        private int victorias;
        private string eleccion;
        #endregion

        #region properties
        public string Nombre
        {
            get { return nombre; }
            set { nombre = value; }
        }

        public string Grupo
        {
            get { return grupo; }
            set { grupo = value; }
        }

        public int Puntos
        {
            get { return puntos; }
            set { puntos = value; }
        }

        public int Victorias
        {
            get { return victorias; }
            set { victorias = value; }
        }

        public string Eleccion
        {
            get { return eleccion; }
            set { eleccion = value; }
        }
        #endregion

        #region constructors
        /// <summary>
        /// constructor defecto
        /// </summary>
        public Jugador()
        {
            nombre = "";
            grupo = "";
            puntos = 0;
            victorias = 0;
            eleccion = "";
        }

        /// <summary>
        /// constructor con params
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="grupo"></param>
        /// <param name="puntos"></param>
        /// <param name="victorias"></param>
        /// <param name="eleccion"></param>
        public Jugador(string nombre, string grupo, int puntos, int victorias, string eleccion)
        {
            this.nombre = nombre;
            this.grupo = grupo;
            this.puntos = puntos;
            this.victorias = victorias;
            this.eleccion = eleccion;
        }
        #endregion
    }
}