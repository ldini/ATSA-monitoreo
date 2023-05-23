$(document).ready(function () {

    $("#filtroTipo, #filtroPrestador, #filtroEstado, #filtroPeriodoMes, #filtroPeriodoAnio").chosen();

    $("#filtroTipo, #filtroPrestador, #filtroEstado, #filtroPeriodoMes, #filtroPeriodoAnio").change(function () {
        var filtroTipo = $("#filtroTipo").val();
        var filtroPrestador = $("#filtroPrestador").val();
        var filtroEstado = $("#filtroEstado").val();
        var filtroPeriodoMes = $("#filtroPeriodoMes").val();
        var filtroPeriodoAnio = $("#filtroPeriodoAnio").val();
        var url = "/monitoreotest/pagos/ObtenerListadoIndex"
            + "?tipoComprobanteId=" + filtroTipo
            + "&prestadorId=" + filtroPrestador
            + "&mesPeriodo=" + filtroPeriodoMes
            + "&anioPeriodo=" + filtroPeriodoAnio
            + "&estado=" + filtroEstado;
        $('#tbl').DataTable().ajax.url(url);
        $('#tbl').DataTable().ajax.reload();
    });

    var oTable = $('#tbl').dataTable({
        "bProcessing": true,
        "bServerSide": true,
        "ajax": {
            "url": "/monitoreotest/pagos/ObtenerListadoIndex",
            "type": "POST"
        },
        "bLengthChange": true,
        "bFilter": true,
        "bSort": true,
        "bInfo": true,
        "order": [[0, "asc"]],
        "aoColumns": [
            { "bSortable": true, "sWidth": '23%' },
            { "bSortable": true, "sWidth": '10%' },
            { "bSortable": true, "sWidth": '11%' },
            { "bSortable": true, "sWidth": '6%' },
            { "bSortable": true, "sWidth": '6%' },
            { "bSortable": true, "sWidth": '12%' },
            { "bSortable": true, "sWidth": '7%' },
            { "bSortable": true, "sWidth": '7%' },
            { "bSortable": false, "sWidth": '5%', "mRender": function (data, type, row) { return RenderPago(data, 1, row[12], row[13]); } },
            { "bSortable": false, "sWidth": '5%', "mRender": function (data, type, row) { return RenderPago(data, 2, row[12], row[13]); } },
            { "bSortable": false, "sWidth": '5%', "mRender": function (data, type, row) { return RenderPago(data, 3, row[12], row[13]); } },
            { "bSortable": false, "sWidth": '3%', "mRender": function (data, type, row) { return RenderObs(data, row[12]); } },
            { "bVisible": false }
        ]
    });
});

function RenderPago(monto, numPago, id, prestadorId) {
    var html = "";
    if (monto == null) {
        html = '<a class="btn btn-warning btn-xs" onclick="SetearMonto(' + id + ', ' + numPago + ', ' + prestadorId + ')" style="width:100%" num-pago="' + numPago + '" id="' + id + '">Pagar ' + numPago + '</a>';
    }
    else {
        html = '<a class="btn btn-success btn-xs" onclick="EliminarPago(' + id + ', ' + numPago + ')" num-pago="' + numPago + '" id="' + id + '">$ ' + monto.toFixed(2).replace(".", ",") + '</a>';
    }
    return html;
}

function SetearMonto(pcomprobanteId, pnumPago, prestadorId) {
    var comprobanteId = pcomprobanteId;
    var numPago = pnumPago;

    GetSaldoCuentaPrestadorSumaFacturas(prestadorId, function (data) {
        var mensajeSaldo = "";
        if (data.saldo < 0) {
            mensajeSaldo = "   [ Suma saldo facturas a favor prestador: $" + (data.saldo * -1).toFixed(2).replace(".", ",") + "] ";
        }
        var monto = prompt("Ingrese monto ($): " + mensajeSaldo, "0,00");
        if (monto == null)
            return;

        var _cid = comprobanteId;
        var _pago = numPago;
        monto = parseFloat(monto.trim().replace(",", "."));

        if (monto == 0) {
            Mensajes.MostrarError("El monto ingresado no puede ser cero.");
            return;
        }

        Mensajes.MostrarSiNo("El monto ingresado es <b>$ " + monto.toFixed(2).replace(".", ",") + "</b>.<br/>¿Es correcto?", function () {
            UpdatePago(_cid, _pago, monto);
        });
    });
}

function EliminarPago(comprobanteId, numPago) {
    var cid = comprobanteId;
    var pago = numPago;
    Mensajes.MostrarSiNo("¿Desea eliminar el pago " + numPago + "?", function () {
        LimpiarPago(cid, pago);
    });
}

function LimpiarPago(comprobanteId, numPago) {
    UpdatePago(comprobanteId, numPago, null);
}

function UpdatePago(comprobanteId, numPago, monto) {
    MostrarCargando();
    $.ajax({
        type: "POST",
        url: '/monitoreotest/pagos/UpdatePago',
        data: JSON.stringify({
            comprobanteId: comprobanteId,
            numPago: numPago,
            monto: monto
        }),
        dataType: "json",
        contentType: "application/json",
        success: function (data) {
            OcultarCargando();
            if (data.success) {
                $("#tbl").DataTable().ajax.reload();
            }
            else {
                Mensajes.MostrarError(data.error);
            }
        }
    });
}

function RenderObs(observaciones, id) {
    var html = "";
    if (observaciones == null || observaciones == "") {
        html = '<a class="btn btn-default btn-xs" onclick="AbrirObs(this)" observacion="" style="width:100%" id="' + id + '"><span class="glyphicon glyphicon-eye-open"></span></a>';
    }
    else {
        html = '<a class="btn btn-primary btn-xs" onclick="AbrirObs(this)" id="' + id + '" observacion="' + observaciones + '" style="width:100%"><span class="glyphicon glyphicon-eye-open"></span></a>';
    }
    return html;
}

function AbrirObs(btnObs) {
    var id = $(btnObs).attr("id");
    var obs = $(btnObs).attr("observacion");
    obs = prompt("Observación:", obs);

    if (obs == null)
        return;

    if (obs.length > 200)
        obs = obs.substring(0, 200);

    MostrarCargando();
    $.ajax({
        type: "POST",
        url: '/monitoreotest/pagos/UpdateObs',
        data: JSON.stringify({
            comprobanteId: id,
            observaciones: obs
        }),
        dataType: "json",
        contentType: "application/json",
        success: function (data) {
            OcultarCargando();
            if (data.success) {
                $("#tbl").DataTable().ajax.reload();
            }
            else {
                Mensajes.MostrarError(data.error);
            }
        }
    });
}
