namespace Proyecto_GolMundial.DTOs.Grupo
{
    public class PosicionEquipo
    {
        public int EquipoId { get; set; }
        public string Nombre { get; set; }
        public int Puntos { get; set; }
        public int PartidosJugados { get; set; }
        public int PartidosGanados { get; set; }
        public int PartidosEmpatados { get; set; }
        public int PartidosPerdidos { get; set; }
        public int GolesAFavor { get; set; }
        public int GolesEnContra { get; set; }
        public int DiferenciaGoles { get; set; }
    }

    public class PosicionesResponse
    {
        public string GrupoCodigo { get; set; }
        public string GrupoNombre { get; set; }
        public List<PosicionEquipo> Posiciones { get; set; } = new List<PosicionEquipo>();
    }
}
