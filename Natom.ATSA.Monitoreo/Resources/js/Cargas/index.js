$(document).ready(function () {
    
    var oTable = $('#tbl').dataTable({
        "bProcessing": true,
        "bServerSide": true,
        "ajax": {
            "url": "/monitoreotest/cargas/ObtenerListadoIndex",
            "type": "POST"
        },
        "bLengthChange": true,
        "bFilter": true,
        "bSort": true,
        "bInfo": true,
        "order": [[6, "desc"]],
        "aoColumns": [
            { "bSortable": true, "sWidth": '14%' },
            { "bSortable": true, "sWidth": '14%' },
            { "bSortable": true, "sWidth": '12%' },
            { "bSortable": true, "sWidth": '24%' },
            { "bSortable": true, "sWidth": '9%' },
            { "bSortable": true, "sWidth": '17%' },
            {
                "bSortable": false, "sWidth": '10%',
                mRender: function (data, type, row) {
                    var html = "";
                    var puede = row[7];
                    var eliminado = row[8];
                    var tienePermisoEliminar = row[9];
                    if (eliminado) {
                        html += '<a class="btn btn-danger btn-xs btn-ver-elimino" id="' + data + '">Anulado (+info)</a>';
                    }
                    else {
                        if (puede && tienePermisoEliminar) {
                            html += '<a class="btn btn-danger btn-xs btn-eliminar" id="' + data + '" ' + (puede ? "puede" : "") + '>Anular</a>';
                        }
                    }
                    return html;
                }
            }
        ]
    });

    $("#tbl").on("click", ".btn-ver-elimino", function (e) {
        var id = e.currentTarget.id;
        $.ajax({
            type: "POST",
            url: '/monitoreotest/cargas/ObtenerInfoAnulacion',
            data: JSON.stringify({
                cargaId: id
            }),
            dataType: "json",
            contentType: "application/json",
            success: function (data) {
                var htmlMensaje = "";
                htmlMensaje += "<b>ANULÓ:</b>&nbsp;" + data.anulo + "<br/>";
                htmlMensaje += "<b>FECHA HORA:</b>&nbsp;" + data.fechaHora + "<br/>";
                htmlMensaje += "<b>MOTIVO:</b>&nbsp;" + data.motivo + "<br/>";
                Mensajes.MostrarOK(htmlMensaje);
            }
        });
    });
    $("#tbl").on("click", ".btn-eliminar", function (e) {
        var id = e.currentTarget.id;
        var puede = $(this).is("[puede]");
        if (puede) {
            Mensajes.MostrarSiNo("¿Desea anular la carga?", function () {
                $("#EliminarCargaId").val(id);
                $("#MotivoModal").modal("show");
                $("#tbl").DataTable().ajax.url("/monitoreotest/cargas/ObtenerListadoIndex");
                $("#tbl").DataTable().ajax.reload();
            });
        }
        else {
            Mensajes.MostrarError("No puedes anular la carga porque ya se encuentra en auditoría.");
        }
    });

    $("#btnMotivoContinuar").click(function () {
        var id = parseInt($("#EliminarCargaId").val());
        var motivo = $("#TextMotivo").val();
        if (motivo.length <= 3) {
            Mensajes.MostrarError("Debes ingresar un motivo.");
            return;
        }
        Eliminar(id, motivo);
    });

    function Eliminar(id, motivo) {
        $.ajax({
            type: "POST",
            url: '/monitoreotest/cargas/EliminarCarga',
            data: JSON.stringify({
                CargaId: id,
                motivo: motivo
            }),
            dataType: "json",
            contentType: "application/json",
            success: function (data) {
                if (data.success) {
                    location.reload();
                }
                else {
                    Mensajes.MostrarError(data.error);
                }
            }
        });
    }
});