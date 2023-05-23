$(document).ready(function () {
    var cargaid = $("#CargaId").val();
    var btnIdAutoIncrement = 0;
    var oTable = $('#tblPendientes').dataTable({
        "bProcessing": true,
        "bServerSide": true,
        "ajax": {
            "url": "/monitoreotest/auditorias/ObtenerListadoFacturas?cargaid=" + cargaid + "&auditada=False",
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
                    var tienePermisoAuditar = row[11];
                    var html = "";
                    if (tienePermisoAuditar) {
                        html += '<a class="btn btn-primary btn-xs btn-auditar btnAuditarFactura" onclick="AbrirModal(' + row[10] + ')" data-facturaid="' + row[10] + '" data-btnid="' + btnIdAutoIncrement + '">Auditar</a>';
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
            "url": "/monitoreotest/auditorias/ObtenerListadoFacturas?cargaid=" + cargaid + "&auditada=True",
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
                    var html = '<a class="btn btn-danger btn-xs btn-anular-auditoria btnAnularAuditoria" onclick="Anular(' + row[10] + ')" data-facturaid="' + row[10] + '">Anular</a>';
                    
                    return html;
                }
            }
        ]
    });

});

function Anular(id) {
    var _id = id;
    Mensajes.MostrarSiNo("¿Desea anular la auditoría de la factura?\nATENCIÓN: La factura volverá a quedar como pendiente de auditar", function () {
        MostrarCargando();
        $.ajax({
            type: "POST",
            url: '/monitoreotest/auditorias/AnularAuditoria',
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
        url: '/monitoreotest/auditorias/ModalCrearAuditoria?facturaid=' + facturaid,
        method: 'GET',
        dataType: 'html',
        success: function (data) {
            $('#ModalDiv').html(data);
            $("#ModalDiv").find("#modalAuditoria").modal('show');
            $("#BtnAuditarId").val(btnid);
            OcultarCargando();
        }
    });
};