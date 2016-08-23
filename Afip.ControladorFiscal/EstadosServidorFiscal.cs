using System;

namespace Afip.ControladorFiscal
{
        /// <summary>
        /// Los c�digos de estado de la aplicaci�n de servidor fiscal (L�zaro gesti�n).
        /// </summary>
	public enum EstadoServidorFiscal
	{
		Esperando = 0,
		Imprimiendo = 1,
		Apagando = 3,
		Reiniciando = 4,
		Error = 5
	}
        
}
