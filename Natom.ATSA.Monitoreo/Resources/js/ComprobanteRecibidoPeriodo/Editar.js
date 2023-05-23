$(document).ready(function () {
    var prestadores = {};

    MostrarCargando();

    setTimeout(function () {
        ArmarGrilla();

        $(".prestadorid").val(0);

        $(".checkAuditor").change(function () {
            var auditor = $(this).attr("auditor");
            $(".checkAuditor:not([auditor='" + auditor + "'])").prop("checked", false);
        });

        LLenarGrilla();

        if ($("#SoloLectura").val() == "True") {
            $("#tbl input:not([type='button']), #tbl select").prop("disabled", true);
            $(".btn-eliminar").remove();
            $("#btnGrabarCRP").remove();
            $(".btn-danger").attr("value", "Volver");
        }

        $(".tipocomprobanteid, .mesperiodo, .anioperiodo, .tipo").chosen();
        //$(".puntodeventa, .numero, .ninterno").ForceNumericOnly();
        $(".ninterno").ForceNumericOnly();
        $('.fecharecibido, .fecha, .fechainformada').datetimepicker({
            format: 'DD/MM/YYYY',
            widgetPositioning: {
                horizontal: 'auto',
                vertical: 'auto'
            }
        });

        if ($("#PuedeAuditar").val() != "True" && $("#SoloLectura").val() != "True")
        {
            $(".tdAuditado").hide();
        }

        var prestadorDOMindex;
        $('.prestador').typeahead({
            minLength: 3,
            items: 20,
            matcher: function (item) { return true; },
            source: function (query, process) {
                prestadores = {};

                prestadorDOMindex = $($($(this.$element)[0]).parents("tr")[0]).attr("index");
                console.log(prestadorDOMindex);

                $.ajax({
                    type: "POST",
                    url: '/monitoreotest/comprobanterecibidoperiodo/ObtenerPrestadores',
                    data: JSON.stringify({
                        prestadores: query
                    }),
                    dataType: "json",
                    contentType: "application/json",
                    success: function (data) {
                        objects = TypeAhead.constructMap(data.datos, prestadores);
                        process(objects);
                    }
                });
            },
            updater: function (item, dom) {
                var data = prestadores[item].label.split(" ///  CUIT: ");
                $("tr[index=" + prestadorDOMindex + "] .prestadorid").val(prestadores[item].id);
                setTimeout(function () {
                    $("tr[index=" + prestadorDOMindex + "] .prestador").val(data[0]);
                }, 100);
                $("tr[index=" + prestadorDOMindex + "] .cuit").val(data[1]);
                return item;
            }
        });

        $(".prestador, .prestadorcuit").on('keypress', function () {
            var index = $($(this).parents("tr")[0]).attr("index");
            $("tr[index=" + index + "] .prestadorid").val(0);
        });

        $(".btn-eliminar").on('click', function () {
            var index = $($(this).parents("tr")[0]).attr("index");
            Mensajes.MostrarSiNo("¿Está seguro que desea eliminar?", function () {
                $("tr[index=" + index + "]").remove();
            });
        });

        $(".monto, .debito").on('keypress', function () {
            var tr = $(this).parents("tr")[0];
            setTimeout(function () {
                var monto = ObtenerDeMoneda($(tr).find(".monto").val()).valor;
                var debito = ObtenerDeMoneda($(tr).find(".debito").val()).valor;
                if (isNaN(monto)) monto = 0;
                if (isNaN(debito)) debito = 0;
                var nApagar = FormatearAMoneda(monto - debito);
                $(tr).find(".apagar").val(nApagar);
            }, 100);
        });

        $(".tipocomprobanteid").change(function () {
            var por = $("#CurrentUser").val();
            var txtPor = $($(this).parents("tr")[0]).find(".liquidadopor");
            if ($(txtPor).val() == "")
                $(txtPor).val(por);
        });

        jQuery(document).on('DOMNodeInserted', "body", function (e) {
            var t = e.target;
            console.log(t);
            if ($(t).is(".dropdown-menu")) {
                var parent = $($(t).parents("td")[0]);
                var d = $(parent).find("input.form-control")[0];                
                var top = $(d).position().top + 25;
                var top = $($("input.prestador")[0]).offset().top * -1 + $(d).position().top + 246;
                $(t).attr("newtop", top.toString());
            }
        });

        setInterval(function () {
            $.each($("[newtop]"), function (i, t) {
                $(t).css("top", $(t).attr("newtop") + "px").css("min-height", "270px");
            });
        }, 150);

        OcultarCargando();
    }, 300);

    setTimeout(function () {
        $(".moneda").change(function () {
            $(".moneda.error").removeClass("error");
            var valor = $(this).val();
            valor = parseFloat(valor.replace("$", "").replace(".", "").replace(",", "."));
            if (isNaN(valor)) {
                $(this).addClass("error");
                return;
            }
            $(this).val(FormatearAMoneda(valor));
        }).trigger("change");
    }, 350);
});

function ArmarGrilla() {
    var html = "";
    var trModelo = $($("#tbl tbody tr[index=0]")[0]).html();
    for (var i = 1; i < 200; i++) {
        html += "<tr index='" + i + "'>" + trModelo + "</tr>";
    }
    $("#tbl tbody").append(html);
}

function FormatearAMoneda(valor, moneda) {
    if (moneda == undefined) {
        moneda = "$";
    }
    return moneda + " " + valor.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&~').replace(".", ",").replace(new RegExp('~', 'g'), '.');
}

function ObtenerDeMoneda(valor) {
    var data = valor.split(" ");
    var moneda = undefined;
    valor = 0;

    if (data.length == 2) {
        moneda = data[0];
        valor = data[1].replace(/\./g, '').replace(",", ".");
    }
    else {
        valor = data[0].replace(/\./g, '').replace(",", ".");
    }

    return {
        moneda: moneda,
        valor: parseFloat(valor)
    };
}

function LLenarGrilla() {
    $.each(comprobantes, function (index, data) {
        $("tr[index='" + index + "']").attr("disabled", data[29]);
        if (data[29]) {
            $("tr[index='" + index + "'] input, tr[index='" + index + "'] select").attr("disabled", true);
        }
        $("tr[index='" + index + "'] .comprobanterecibidoid").val(data[0]);
        $("tr[index='" + index + "'] .fecharecibido").val(data[1]);
        $("tr[index='" + index + "'] .prestador").val(data[2]);
        $("tr[index='" + index + "'] .cuit").val(data[3]);
        $("tr[index='" + index + "'] .prestadorid").val(data[4]);
        $("tr[index='" + index + "'] .tipocomprobanteid").val(data[5]);
        $("tr[index='" + index + "'] .puntodeventa").val(data[6]);
        $("tr[index='" + index + "'] .numero").val(data[7]);
        $("tr[index='" + index + "'] .fecha").val(data[8]);
        $("tr[index='" + index + "'] .ninterno").val(data[9]);
        $("tr[index='" + index + "'] .mesperiodo").val(data[10]);
        $("tr[index='" + index + "'] .anioperiodo").val(data[11]);
        $("tr[index='" + index + "'] .refact").prop("checked", data[12]);
        $("tr[index='" + index + "'] .monto").val(data[13] != null ? data[13].toFixed(2).replace(".", ",") : null);
        $("tr[index='" + index + "'] .debito").val(data[14] != null ? data[14].toFixed(2).replace(".", ",") : null);
        $("tr[index='" + index + "'] .apagar").val(data[15] != null ? data[15].toFixed(2).replace(".", ",") : null);
        $("tr[index='" + index + "'] .tiposoportemag").prop("checked", data[16]);
        $("tr[index='" + index + "'] .ambul").prop("checked", data[17]);
        $("tr[index='" + index + "'] .int").prop("checked", data[18]);
        $("tr[index='" + index + "'] .montoint").val(data[19] != null ? data[19].toFixed(2).replace(".", ",") : null);
        $("tr[index='" + index + "'] .tipo").val(data[20]);
        $("tr[index='" + index + "'] .observaciones").val(data[21]);
        $("tr[index='" + index + "'] .consiliado").prop("checked", data[22]);
        $("tr[index='" + index + "'] .liquidado").prop("checked", data[23]);
        $("tr[index='" + index + "'] .liquidadopor").val(data[24]);
        $("tr[index='" + index + "'] .babich").prop("checked", data[25]);
        $("tr[index='" + index + "'] .gt").prop("checked", data[26]);
        $("tr[index='" + index + "'] .mt").prop("checked", data[27]);
        $("tr[index='" + index + "'] .fechainformada").val(data[28]);
        $("tr[index='" + index + "'] .auditado").prop("checked", data[30]);
        $("tr[index='" + index + "'] .pmif").prop("checked", data[31]);
    });
}

function Cancelar() {
    if ($("#SoloLectura").val() == "True") {
        location.href = '/monitoreotest/ComprobanteRecibidoPeriodo';
        return;
    }

    Mensajes.MostrarSiNo("¿Desea cancelar?", function () {
        location.href = '/monitoreotest/ComprobanteRecibidoPeriodo';
    });
}

function ObtenerData() {
    var comprobantes = [];

    $("tr[index]:not([disabled])").each(function (i, tr) {
        var domTR = $(tr)[0];
        var index = $(this).attr("index");
        var puntodeventa = $(domTR).find(".puntodeventa").val();
        var numero = $(domTR).find(".numero").val();
        var tipoComprobanteId = $(domTR).find(".tipocomprobanteid").val();
        var tipo = $(domTR).find(".tipo").val();
        var monto = ObtenerDeMoneda($(tr).find(".monto").val()).valor;
        var debito = ObtenerDeMoneda($(tr).find(".debito").val()).valor;
        if (isNaN(monto)) monto = 0;
        if (isNaN(debito)) debito = 0;
        var montoint = ObtenerDeMoneda($(tr).find(".montoint").val()).valor;

        if (/*puntodeventa !== "" || */numero !== "" || tipoComprobanteId !== "0") {
            var comprobante = {
                index: index,
                ComprobanteRecibidoId: $(domTR).find(".comprobanterecibidoid").val(),
                ComprobanteRecibidoPeriodoId: 0,
                FechaRecibido: $(domTR).find(".fecharecibido").val(),
                PrestadorId: $(domTR).find(".prestadorid").val(),
                PrestadorRazonSocial: $(domTR).find(".prestador").val(),
                PrestadorCUIT: $(domTR).find(".cuit").val(),
                TipoComprobanteId: tipoComprobanteId,
                Fecha: $(domTR).find(".fecha").val(),
                PuntoDeVenta: puntodeventa,
                Numero: numero,
                NInterno: $(domTR).find(".ninterno").val(),
                MesPeriodo: $(domTR).find(".mesperiodo").val(),
                AnioPeriodo: $(domTR).find(".anioperiodo").val(),
                Refact: $(domTR).find(".refact").is(":checked"),
                Monto: monto,
                Debito: debito,
                APagar: monto - debito,
                TipoSoporteMag: $(domTR).find(".tiposoportemag").is(":checked"),
                Ambul: $(domTR).find(".ambul").is(":checked"),
                Int: $(domTR).find(".int").is(":checked"),
                MontoInt: montoint,
                Tipo: tipo,
                Observaciones: $(domTR).find(".observaciones").val(),
                Consiliado: $(domTR).find(".consiliado").is(":checked"),
                Liquidado: $(domTR).find(".liquidado").is(":checked"),
                LiquidadoPor: $(domTR).find(".liquidadopor").val(),
                Babich: $(domTR).find(".babich").is(":checked"),
                GT: $(domTR).find(".gt").is(":checked"),
                MT: $(domTR).find(".mt").is(":checked"),
                FechaInformada: $(domTR).find(".fechainformada").val(),
                Auditado: $(domTR).find(".auditado").is(":checked"),
                InformadoEnPMIF: $(domTR).find(".pmif").is(":checked")
            };


            comprobantes.push(comprobante);
        }
    });

    return comprobantes;
}

function Grabar() {
    var comprobantes = ObtenerData();
    var valido = true;
    $("tr[index] .error").removeClass("error");

    if (comprobantes.length == 0) {
        Mensajes.MostrarError("Debes ingresar al menos un comprobante.");
        return;
    }

    $.each(comprobantes, function (i, c) {
        if (!ComprobanteValido(c)) {
            valido = false;
            return;
        }
    });

    if (valido) {
        MostrarCargando();
        $.ajax({
            type: "POST",
            url: '/monitoreotest/comprobanterecibidoperiodo/Grabar',
            data: JSON.stringify({
                comprobanteRecibidoPeriodoId: $("#ComprobanteRecibidoPeriodoId").val(),
                comprobantes: comprobantes
            }),
            dataType: "json",
            contentType: "application/json",
            success: function (data) {
                if (data.success) {
                    location.href = '/monitoreotest/ComprobanteRecibidoPeriodo';
                }
                else {
                    Mensajes.MostrarError(data.error);
                    OcultarCargando();
                }
            }
        });
    }
}

function ComprobanteValido(comprobante) {
    if (comprobante.FechaRecibido == "") {
        Mensajes.MostrarError("Falta seleccionar Fecha de recibido.");
        $("tr[index='" + comprobante.index + "'] .fecharecibido").addClass("error").focus();
        return false;
    }
    if (comprobante.PrestadorRazonSocial == "") {
        Mensajes.MostrarError("Falta indicar Prestador.");
        $("tr[index='" + comprobante.index + "'] .prestador").addClass("error").focus();
        return false;
    }
    if (comprobante.PrestadorCUIT == "") {
        Mensajes.MostrarError("Falta indicar Prestador CUIT.");
        $("tr[index='" + comprobante.index + "'] .cuit").addClass("error").focus();
        return false;
    }
    if (comprobante.TipoComprobanteId == "0") {
        Mensajes.MostrarError("Falta indicar Tipo de comprobante.");
        $("tr[index='" + comprobante.index + "'] .tipocomprobanteid").trigger("chosen:selected");
        return false;
    }
    //if (comprobante.PuntoDeVenta == "") {
    //    Mensajes.MostrarError("Falta indicar Punto de venta.");
    //    $("tr[index='" + comprobante.index + "'] .puntodeventa").addClass("error").focus();
    //    return false;
    //}
    //if (parseInt(comprobante.PuntoDeVenta) < 1 || parseInt(comprobante.PuntoDeVenta) > 9999) {
    //    Mensajes.MostrarError("Punto de venta inválido.");
    //    $("tr[index='" + comprobante.index + "'] .puntodeventa").addClass("error").focus();
    //    return false;
    //}
    if (comprobante.Numero == "") {
        Mensajes.MostrarError("Falta indicar Número.");
        $("tr[index='" + comprobante.index + "'] .numero").addClass("error").focus();
        return false;
    }
    //if (parseInt(comprobante.Numero) < 1) {
    //    Mensajes.MostrarError("Número inválido.");
    //    $("tr[index='" + comprobante.index + "'] .numero").addClass("error").focus();
    //    return false;
    //}
    if (comprobante.Monto == 0) {
        Mensajes.MostrarError("Monto inválido.");
        $("tr[index='" + comprobante.index + "'] .monto").addClass("error").focus();
        return false;
    }
    //if (comprobante.Int == true && isNaN(comprobante.MontoInt)) {
    //    Mensajes.MostrarError("Monto interfilial inválido.");
    //    $("tr[index='" + comprobante.index + "'] .montoint").addClass("error").focus();
    //    return false;
    //}
    if (comprobante.Liquidado && comprobante.LiquidadoPor == "") {
        Mensajes.MostrarError("Falta indicar Quién liquidó.");
        $("tr[index='" + comprobante.index + "'] .liquidadopor").addClass("error").focus();
        return false;
    }
    //if (comprobante.FechaInformada == 0) {
    //    Mensajes.MostrarError("Falta seleccionar 'Fecha informa'.");
    //    $("tr[index='" + comprobante.index + "'] .fechainformada").addClass("error").focus();
    //    return false;
    //}
    return true;
}

