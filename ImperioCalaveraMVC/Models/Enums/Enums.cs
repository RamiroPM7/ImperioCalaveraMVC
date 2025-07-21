namespace ImperioCalaveraMVC.Models.Enums
{
    public class Enums
    {
        public enum Role
        {
            Cliente = 0,
            Barbero = 1,
            Admin = 2
        }

        public enum EstadoCita
        {
            Pendiente,     // Programada pero aún no iniciada
            Iniciada,      // El barbero marcó como iniciada
            Finalizada,    // El barbero la terminó
            Cancelada,     // Cancelada por cliente, barbero o administrador
        }


      /*  public enum BloqueoTipo
        {
            Vacaciones = 0,
            Descanso = 1
        }*/

        public enum ReporteTipo
        {
            Semanal = 0,
            Mensual = 1
        }

       


    }
}
