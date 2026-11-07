namespace ASOFOTEC.Helpers
{
    public static class FeedHelper
    {
        public static string Iniciales(string? nombre)
        {
            var partes = (nombre ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return partes.Length == 0
                ? "?"
                : string.Concat(partes.Take(2).Select(p => char.ToUpper(p[0])));
        }

        // Número de 0 a 7 calculado con el nombre: cada persona conserva siempre el mismo color de avatar
        public static int ColorAvatar(string? nombre)
        {
            var letras = new string((nombre ?? string.Empty).Where(char.IsLetter).ToArray()).ToLowerInvariant();
            if (letras.Length == 0) return 0;

            var acumulado = 0;
            foreach (var letra in letras)
            {
                acumulado = (acumulado * 31 + letra) % 1000003;
            }
            return acumulado % 8;
        }

        public static string TiempoRelativo(DateTime fecha)
        {
            var dif = DateTime.Now - fecha;

            if (dif.TotalMinutes < 1) return "ahora";
            if (dif.TotalMinutes < 60) return $"hace {(int)dif.TotalMinutes} min";
            if (dif.TotalHours < 24) return $"hace {(int)dif.TotalHours} h";
            if (dif.TotalDays < 7) return $"hace {(int)dif.TotalDays} d";
            return fecha.ToString("dd/MM/yyyy");
        }

        public static string Resumir(string? texto, int max)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;
            return texto.Length > max ? texto.Substring(0, max).TrimEnd() + "..." : texto;
        }
    }
}
