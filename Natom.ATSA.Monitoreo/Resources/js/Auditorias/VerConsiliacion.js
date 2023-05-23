$(document).ready(function () {
    var cargaid = $("#CargaId").val();
    var btnIdAutoIncrement = 0;
    var oTable = $('#tblPendientes').dataTable({
        "bProcessing": true,
        "bServerSide": true,
        "ajax": {
            "url": "/monitoreotest/auditorias/ObtenerListadoFacturasConsiliacion?cargaid=" + cargaid + "&consiliada=False",
            "type": "POST"
        },
        "bLengthChange": true,
        "bFilter": true,
        "bSort": true,
        "bInfo": true,
        "order": [[9, "desc"]],
        "aoColumns": [
            { "bSortable": true, "sWidth": '10%' },
            { "bSortable": true, "sWidth": '4%' },
            { "bSortable": true, "sWidth": '4%' },
            { "bSortable": true, "sWidth": '12%' },
            { "bSortable": true, "sWidth": '23%' },
            { "bSortable": true, "sWidth": '4%' },
            { "bSortable": true, "sWidth": '3%' },
            { "bSortable": true, "sWidth": '19%' },
            { "bSortable": true, "sWidth": '8%' },
            { "bSortable": true, "sWidth": '5%' },
            {
                "bSortable": false, "sWidth": '8%',
                mRender: function (data, type, row) {
                    var tienePermisoConsiliacion = row[11];
                    var html = "";
                    if (tienePermisoConsiliacion) {
                        html += '<a class="btn btn-warning btn-xs btn-auditar btnConsiliarFactura" onclick="AbrirModal(' + row[10] + ')" data-facturaid="' + row[10] + '" data-btnid="' + btnIdAutoIncrement + '">Consiliar</a>';
                    }
                    btnIdAutoIncrement++;
                    return html;
                }
            }
        ]
    });

    var aTable = $('#tblAuditados').dataTable({
        "bProcessing": true,
        "bServerSide": true,
        "ajax": {
            "url": "/monitoreotest/auditorias/ObtenerListadoFacturasConsiliacion?cargaid=" + cargaid + "&consiliada=True",
            "type": "POST"
        },
        "bLengthChange": true,
        "bFilter": true,
        "bSort": true,
        "bInfo": true,
        "order": [[9, "desc"]],
        "aoColumns": [
            { "bSortable": true, "sWidth": '10%' },
            { "bSortable": true, "sWidth": '4%' },
            { "bSortable": true, "sWidth": '4%' },
            { "bSortable": true, "sWidth": '12%' },
            { "bSortable": true, "sWidth": '23%' },
            { "bSortable": true, "sWidth": '4%' },
            { "bSortable": true, "sWidth": '3%' },
            { "bSortable": true, "sWidth": '21%' },
            { "bSortable": true, "sWidth": '8%' },
            { "bSortable": true, "sWidth": '5%' },
            {
                "bSortable": false, "sWidth": '6%',
                mRender: function (data, type, row) {
                    var html = '<a class="btn btn-danger btn-xs btn-anular-auditoria btnAnularConsiliacion" onclick="Anular(' + row[10] + ')" data-facturaid="' + row[10] + '">Anular</a>';
                    
                    return html;
                }
            }
        ]
    });

});

function Anular(id) {
    var _id = id;
    Mensajes.MostrarSiNo("¿Desea anular la consiliación de la factura?\nATENCIÓN: La factura volverá a quedar como pendiente de consiliar", function () {
        MostrarCargando();
        $.ajax({
            type: "POST",
            url: '/monitoreotest/auditorias/AnularConsiliacion',
            data: JSON.stringify({
                facturaId: _id
            }),
            dataType: "json",
            contentType: "application/json",
            success: function (data) {
                OcultarCargando();
                if (data.success) {
                    $("#tblPendientes").DataTable().ajax.reload();
                    $("#tblAuditados").DataTable().ajax.reload();
                }
                else {
                    Mensajes.MostrarError(data.error);
                }
            }
        });
    });
}

function AbrirModal(e) {
    var facturaid = e;
    var btnid = $(this).attr('data-btnid');
    MostrarCargando();
    $.ajax({
        url: '/monitoreotest/auditorias/ModalCrearConsiliacion?facturaid=' + facturaid,
        method: 'GET',
        dataType: 'html',
        success: function (data) {
            $('#ModalDiv').html(data);
            $("#ModalDiv").find("#modalConsiliacion").modal('show');
            $("#BtnAuditarId").val(btnid);
            OcultarCargando();
        }
    });
};