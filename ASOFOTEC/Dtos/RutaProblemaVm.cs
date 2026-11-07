namespace ASOFOTEC.Dtos
{
    public class MiembroDto
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool EsLider { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class AvanceDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public int? Porcentaje { get; set; }
        public DateTime Fecha { get; set; }
    }

    // Todo lo que necesita la pantalla del problema para los pasos 2 al 7
    public class RutaProblemaVm
    {
        public int ProblemaId { get; set; }
        public int Etapa { get; set; } = 1;

        // Quién está mirando la página
        public bool Autenticado { get; set; }
        public bool EsDueno { get; set; }          // creó el problema
        public bool EsLider { get; set; }          // creó la solución seleccionada
        public bool EsColaborador { get; set; }    // se unió al equipo
        public bool EsMiembro => EsLider || EsColaborador;
        public bool PuedeGestionar => EsDueno || EsMiembro;

        // Paso 2
        public string? Causas { get; set; }
        public string? Consecuencias { get; set; }
        public bool CausasDocumentadas =>
            !string.IsNullOrWhiteSpace(Causas) || !string.IsNullOrWhiteSpace(Consecuencias);

        // Pasos 4 al 7: solución elegida
        public SolucionDto? Seleccionada { get; set; }
        public List<MiembroDto> Equipo { get; set; } = new();
        public List<AvanceDto> Avances { get; set; } = new();
        public int Progreso { get; set; }

        // Paso 7
        public bool Resuelto => Seleccionada?.Estado == "Implementada";
        public string? Resultados { get; set; }
        public DateTime? FechaResultados { get; set; }

        // Siguiente paso sugerido: texto y paso al que lleva el botón
        public (string Mensaje, int Paso) SiguientePaso()
        {
            if (Resuelto) return ("¡Problema resuelto! Revisa los resultados documentados.", 7);

            return Etapa switch
            {
                6 => PuedeGestionar
                        ? ("La solución está en marcha. Comparte un nuevo avance o documenta los resultados finales.", 6)
                        : ("El equipo está trabajando en la solución. Mira cómo va.", 6),
                5 => PuedeGestionar
                        ? ("Ya hay una solución elegida. Suma colaboradores y registra el primer avance.", 5)
                        : ("La solución ya fue elegida. Únete al equipo para colaborar.", 5),
                4 => EsDueno
                        ? ("Ya hay varias propuestas: compáralas y elige la mejor.", 4)
                        : ("Compara las propuestas y apoya la mejor con tu 👍.", 4),
                3 => ("Ya hay propuestas. Apoya la que más te convenza o aporta la tuya.", 3),
                2 => ("El problema está documentado. ¡Propón la primera solución!", 3),
                _ => EsDueno
                        ? ("Documenta las causas y consecuencias para que la comunidad entienda mejor el problema.", 2)
                        : ("Sé el primero en proponer una solución.", 3)
            };
        }
    }
}
