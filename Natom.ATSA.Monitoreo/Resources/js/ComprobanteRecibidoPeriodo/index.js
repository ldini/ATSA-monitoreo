$(document).ready(function () {

    var oTable = $('#tbl').dataTable({
        "bProcessing": true,
        "bServerSide": true,
        "ajax": {
            "url": "/monitoreotest/comprobanterecibidoperiodo/ObtenerListadoIndex",
            "type": "POST"
        },
        "bLengthChange": true,
        "bFilter": true,
        "bSort": true,
        "bInfo": true,
        "order": [[0, "desc"]],
        "aoColumns": [
            { "bSortable": true, "sWidth": '20%' },
            { "bSortable": true, "sWidth": '18%' },
            { "bSortable": true, "sWidth": '18%' },
            { "bSortable": true, "sWidth": '24%' },
            {
                "bSortable": false, "sWidth": '20%',
                mRender: function (data, type, row) {
                    var html = "";
                    var eliminado = row[5];
                    var tienePermisoEditar = row[6];
                    var tienePermisoEliminar = row[7];
                    var puedeEditarlo = row[8];
                    var esAdmin = row[9];
                            
                    if (eliminado) {
                        if (esAdmin) {
                            html += '&nbsp;<a class="btn btn-info btn-xs btn-visualizar" id="' + data + '">Visualizar</a>';
                        }
                        html += '&nbsp;<a class="btn btn-danger btn-xs btn-ver-elimino" id="' + data + '">Anulado (+info)</a>';
                    }
                    else {
                        html += '&nbsp;<a class="btn btn-info btn-xs btn-visualizar" id="' + data + '">Visualizar</a>';
                        
                        if (tienePermisoEditar && puedeEditarlo) {
                            html += '&nbsp;<a class="btn btn-primary btn-xs btn-editar" id="' + data + '">Editar</a>';
                        }
                        if (tienePermisoEliminar) {
                            html += '&nbsp;<a class="btn btn-danger btn-xs btn-eliminar" id="' + data + '">Anular</a>';
                        }
                    }
                    html += '&nbsp;<a class="btn btn-info btn-xs btn-imprimir" id="' + data + '">Imprimir</a>';
                    return html;
                }
            }
        ]
    });

    $("#tbl").on("click", ".btn-imprimir", function (e) {
        var id = e.currentTarget.id;
        location.href = "/monitoreotest/comprobanterecibidoperiodo/Imprimir?id=" + id + "&sv=a";      
    });

    $("#tbl").on("click", ".btn-ver-elimino", function (e) {
        var id = e.currentTarget.id;
        $.ajax({
            type: "POST",
            url: '/monitoreotest/comprobanterecibidoperiodo/ObtenerInfoAnulacion',
            data: JSON.stringify({
                id: id
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
        Mensajes.MostrarSiNo("¿Desea anular el período?\nATENCIÓN: Se anularán también sus facturas cargadas.", function () {
            $("#EliminarPeriodoId").val(id);
            $("#MotivoModal").modal("show");
        });

    });

    $("#tbl").on("click", ".btn-editar", function (e) {
        var id = e.currentTarget.id;
        location.href = "/monitoreotest/comprobanterecibidoperiodo/Editar?id=" + id;
    });

    $("#tbl").on("click", ".btn-visualizar", function (e) {
        var id = e.currentTarget.id;
        location.href = "/monitoreotest/comprobanterecibidoperiodo/Editar?id=" + id + "&sv=a";
    });

   

    $("#btnMotivoContinuar").click(function () {
        var id = parseInt($("#EliminarPeriodoId").val());
        var motivo = $("#TextMotivo").val();
        if (motivo.length <= 3) {
            Mensajes.MostrarError("Debes ingresar un motivo.");
            return;
        }
        Eliminar(id, motivo);
    });

    function Eliminar(id, motivo) {
        MostrarCargando();
        $.ajax({
            type: "POST",
            url: '/monitoreotest/comprobanterecibidoperiodo/AnularPeriodo',
            data: JSON.stringify({
                id: id,
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
                    OcultarCargando();
                }
            }
        });
    }
});