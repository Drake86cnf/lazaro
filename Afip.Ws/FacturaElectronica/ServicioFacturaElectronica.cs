﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Afip.Ws.AfipAutenticacion;
using Afip.Ws.AfipFe;

namespace Afip.Ws.FacturaElectronica
{
        /// <summary>
        /// Cliente del servicio web de facturación electrónica WSFEv1.
        /// </summary>
        public class ServicioFacturaElectronica : ServicioAfip
        {
                /// <summary>
                /// El CUIT del cliente del servicio web.
                /// </summary>
                public string Cuit { get; set; }

                /// <summary>
                /// El ticket de acceso al servicio web.
                /// </summary>
                public Autenticacion.TicketAcceso TicketAcceso { get; set; }

                public ServicioFacturaElectronica()
                { }

                public ServicioFacturaElectronica(string cuit, Autenticacion.TicketAcceso ta)
                        : this()
                {
                        this.Cuit = cuit;
                        this.TicketAcceso = ta;
                }

                /// <summary>
                /// Consulta el estado de los servicios web de AFIP.
                /// </summary>
                /// <returns>True si los servicios están funcionando</returns>
                public bool ProbarEstadoServicios()
                {
                        var Clie = new ServiceSoapClient();
                        var Estado = Clie.FEDummy();

                        return Estado.AppServer == "OK" && Estado.AuthServer == "OK"; // && Estado.DbServer == "OK";
                }

                /// <summary>
                /// Devuelve True si tiene un ticket de acceso vigente.
                /// </summary>
                public bool TieneTicketDeAccesoValido()
                {
                        return this.TicketAcceso != null && this.TicketAcceso.EsValido();
                }

                /// <summary>
                /// Obtiene una lista de los tipos de comprobante admitidos por el servicio web.
                /// </summary>
                public CbteTipo[] ObtenerTiposDeComprobante()
                {
                        var Clie = new ServiceSoapClient();
                        var Res = Clie.FEParamGetTiposCbte(this.CrearFEAuthRequest());

                        return Res.ResultGet;
                }

                /// <summary>
                /// Obtiene una lista de los conceptos admitidos por el servicio web.
                /// </summary>
                public ConceptoTipo[] ObtenerConceptos()
                {
                        var Clie = new ServiceSoapClient();
                        var Res = Clie.FEParamGetTiposConcepto(this.CrearFEAuthRequest());

                        return Res.ResultGet;
                }

                /// <summary>
                /// Obtiene los datos del último comprobante autorizado para un PV.
                /// </summary>
                /// <param name="pv">El punto de venta a consultar.</param>
                /// <param name="tipoComprob">El tipo de comprobante a consultar.</param>
                public FERecuperaLastCbteResponse UltimoComprobante(int pv, Tablas.ComprobantesTipos tipoComprob)
                {
                        var Clie = new ServiceSoapClient();

                        var Res = Clie.FECompUltimoAutorizado(this.CrearFEAuthRequest(), pv, (int)tipoComprob);

                        return Res;
                }

                /// <summary>
                /// Solicitar un CAE para uno o más comprobantes.
                /// </summary>
                /// <param name="solCae">Los datos para la solicitud del CAE.</param>
                /// <returns>True si la solicitud tuvo éxito total o parcial, False si no tuvo éxito.</returns>
                public bool SolictarCae(SolicitudCae solCae)
                {
                        var Clie = new ServiceSoapClient();

                        var DetallesComprobantes = new FECAEDetRequest[solCae.Comprobantes.Count];

                        int i = 0;
                        foreach (ComprobanteAsociado Comprob in solCae.Comprobantes) {
                                var DetalleComprobante = new FECAEDetRequest()
                                {
                                        Concepto = (int)Comprob.Conceptos,
                                        DocTipo = (int)Comprob.Cliente.DocumentoTipo,
                                        DocNro = Comprob.Cliente.DocumentoNumero,
                                        CbteDesde = Comprob.Numero,
                                        CbteHasta = Comprob.Numero,
                                        CbteFch = DateTime.Now.ToString("yyyyMMdd"),
                                        ImpTotal = decimal.ToDouble(Comprob.ImporteTotal()),
                                        ImpTotConc = decimal.ToDouble(Comprob.ImporteNetoNoGravado),
                                        ImpNeto = decimal.ToDouble(Comprob.ImporteNetoGravado),
                                        ImpOpEx = decimal.ToDouble(Comprob.ImporteExento),
                                        ImpIVA = decimal.ToDouble(Comprob.ImporteIva()),
                                        //ImpTrib = Comprob.TotalTributos(),
                                        MonId = "PES",
                                        MonCotiz = 1,
                                        /* CbtesAsoc = new CbteAsoc[1]
                                        {
                                                new CbteAsoc()
                                                {
                                                        Nro = 0,
                                                        PtoVta = 0,
                                                        Tipo = Tablas.ComprobantesTipos.NotaDeCreditoA
                                                }
                                        } */
                                };

                                // Si es un comprobante con servicios, agregar los campos obligatorios
                                if ((Comprob.Conceptos | Tablas.Conceptos.Servicios) == Tablas.Conceptos.Servicios) {
                                        DetalleComprobante.FchServDesde = Comprob.ServicioFechaDesde.ToString("yyyyMMdd");
                                        DetalleComprobante.FchServHasta = Comprob.ServicioFechaHasta.ToString("yyyyMMdd");
                                        DetalleComprobante.FchVtoPago = Comprob.FechaVencimientoPago.ToString("yyyyMMdd");
                                }

                                // Agregar la tabla de alícuotas
                                if (Comprob.ImportesAlicuotas != null && Comprob.ImportesAlicuotas.Count > 0) {
                                        DetalleComprobante.Iva = new AlicIva[Comprob.ImportesAlicuotas.Count];
                                        int j = 0;
                                        foreach (ImporteAlicuota Alic in Comprob.ImportesAlicuotas) {
                                                DetalleComprobante.Iva[j++] = new AlicIva()
                                                {
                                                        Id = (int)Alic.Alicuota,
                                                        BaseImp = decimal.ToDouble(Alic.BaseImponible),
                                                        Importe = decimal.ToDouble(Alic.Importe)
                                                };
                                        }
                                }

                                DetallesComprobantes[i++] = DetalleComprobante;
                        }


                        // Crear la solicitud
                        var CaeReq = new FECAERequest()
                        {
                                FeCabReq = new FECAECabRequest()
                                {
                                        CantReg = solCae.Comprobantes.Count,
                                        PtoVta = solCae.PuntoDeVenta,
                                        CbteTipo = (int)solCae.TipoComprobante,
                                },
                                FeDetReq = DetallesComprobantes
                        };

                        // Llamar al WS para hacer la solicitud
                        var Res = Clie.FECAESolicitar(this.CrearFEAuthRequest(), CaeReq);

                        solCae.Observaciones = new List<Observacion>();
                        /* if (Res.FeCabResp.Resultado == "R") {
                                // Todo el lote rechazado
                                foreach (var Er in Res.Errors) {
                                        solCae.Observaciones.Add(new Observacion(Er.Code, Er.Msg));
                                }
                        } else { */
                                // Aprobado total o parcial
                                int ci = 0;
                                foreach (var De in Res.FeDetResp) {
                                        var Comprob = solCae.Comprobantes[ci++];
                                        Comprob.Cae = new Cae();
                                        if (De.Resultado == "A") {
                                                // Aprobado
                                                Comprob.Cae.CodigoCae = De.CAE;
                                                Comprob.Cae.Vencimiento = DateTime.ParseExact(De.CAEFchVto, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                                                Comprob.Numero = System.Convert.ToInt32(De.CbteDesde);
                                        }

                                        if(De.Observaciones != null && De.Observaciones.Count<Obs>() > 0) {
                                                Comprob.Obs = new Observacion(De.Observaciones[0].Code, De.Observaciones[0].Msg, Comprob);
                                                foreach (var Er in De.Observaciones) {
                                                        solCae.Observaciones.Add(new Observacion(Er.Code, Er.Msg, Comprob));
                                                }
                                        }
                                }
                        /* } */
                        
                        if (Res.FeCabResp.Resultado != "R") {
                                return true;
                        } else {
                                return false;
                        }
                }


                private FEAuthRequest CrearFEAuthRequest()
                {
                        var Res = new FEAuthRequest();

                        Res.Cuit = long.Parse(this.Cuit.Replace("-", "").Replace(" ", "").Replace(".", ""));
                        Res.Sign = this.TicketAcceso.Sign;
                        Res.Token = this.TicketAcceso.Token;

                        return Res;
                }

                /// <summary>
                /// Calcula el dígito verificador del código de identificación que se imprime en código
                /// de barras en los comprobantes de AFIP.
                /// http://www.afip.gov.ar/afip/resol170204.html
                /// </summary>
                /// <param name="codigo">El código de identificación, sin el digito verificador.</param>
                /// <returns>El digito verificador.</returns>
                public static int DigitoVerificadorIdentificacionComprobante(string codigo)
                {
                        int Sumatoria = 0;
                        // Sumar todos los dígitos en posiciones impares
                        for (int i = 0; i <= codigo.Length - 1; i += 2) 
                        {
                                Sumatoria += int.Parse(codigo.Substring(i, 1));
                        }

                        // Multiplicar por 3
                        Sumatoria *= 3;

                        // Sumar todos los dígitos en posiciones pares
                        for (int i = 1; i <= codigo.Length - 1; i += 2) {
                                Sumatoria += int.Parse(codigo.Substring(i, 1));
                        }

                        // Buscar la diferencia con el próximo múltiplo de 10
                        // Por ejemplo, si acá tengo un 88, el resultado es 2, si tengo 54 el resultado es 6, etc.
                        int ProximoMultiploDe10 = decimal.ToInt32(Math.Ceiling(Sumatoria / 10m) * 10);
                        int Resultado = ProximoMultiploDe10 - Sumatoria;
                        // También podría ser 10 - (N módulo de 10) si N > 0

                        return Resultado;
                }
        }
}
