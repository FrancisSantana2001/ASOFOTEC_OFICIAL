namespace ASOFOTEC.Helpers
{
    // La "ruta" de un problema en ASOFOTEC: de la descripción a los resultados documentados
    public static class RutaProblemaHelper
    {
        public const int TotalPasos = 7;

        // Nombre corto de cada paso (índice 0 = paso 1)
        public static readonly string[] Pasos =
            { "Describir", "Causas", "Proponer", "Comparar", "Equipo", "Avances", "Resultados" };

        // Cómo se llama el estado del problema según el paso en que va
        public static readonly string[] Estados =
        {
            "",
            "Publicado",
            "Documentado",
            "Recibiendo soluciones",
            "Comparando propuestas",
            "Formando equipo",
            "En ejecución",
            "Resuelto"
        };

        // Paso en el que está el problema ahora mismo, según lo que ya existe
        public static int CalcularEtapa(bool causasDocumentadas, int totalSoluciones,
                                        bool seleccionada, int totalAvances, bool resuelto)
        {
            if (resuelto) return 7;
            if (seleccionada && totalAvances > 0) return 6;
            if (seleccionada) return 5;
            if (totalSoluciones >= 2) return 4;
            if (totalSoluciones >= 1) return 3;
            if (causasDocumentadas) return 2;
            return 1;
        }

        public static string NombreEstado(int etapa) =>
            etapa >= 1 && etapa <= TotalPasos ? Estados[etapa] : string.Empty;

        public static int Porcentaje(int etapa) =>
            (int)Math.Round(etapa * 100.0 / TotalPasos);

        // Clase de color del estado (definida en wwwroot/css/tema.css): ruta-1 ... ruta-7.
        // Cada paso de la ruta tiene siempre el mismo color en todo el sitio.
        public static string Color(int etapa) =>
            etapa >= 1 && etapa <= TotalPasos ? $"ruta-{etapa}" : "ruta-1";
    }
}
