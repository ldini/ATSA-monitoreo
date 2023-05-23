$(document).ready(function () {
    var oTable = $('#tbl').dataTable({
        "bProcessing": true,
        "bServerSide": true,
        "ajax": {
            "url": "/monitoreotest/usuarios/ObtenerListadoIndex",
            "type": "POST"
        },
        "bLengthChange": true,
        "bFilter": true,
        "bSort": true,
        "bInfo": true,
        "order": [[0, "asc"]],
        "aoColumns": [
            { "bSortable": true, "sWidth": '28%' },
            { "bSortable": true, "sWidth": '30%' },
            { "bSortable": true, "sWidth": '20%' },
            { "bSortable": false, "sWidth": '17%',
                mRender: function (data, type, row) {
                    var html = "";
                    html += '<a class="btn btn-primary btn-xs btn-editar" id="' + data + '">Editar</a>';
                    html += '&nbsp;<a class="btn btn-danger btn-xs btn-eliminar" id="' + data + '">Eliminar</a>';
                    return html;
                }
            }
        ]
    });

    $("#tbl").on("click", ".btn-editar", function (e) {
        var id = e.currentTarget.id;
        location.href = "/monitoreotest/usuarios/Edit?id=" + id.toString();
    });

    $("#tbl").on("click", ".btn-eliminar", function (e) {
        var id = e.currentTarget.id;
        Mensajes.MostrarSiNo("¿Eliminar el usuario?", function () {
            $.ajax({
                type: "POST",
                url: '/monitoreotest/usuarios/Eliminar',
                data: JSON.stringify({
                    id: id
                }),
                dataType: "json",
                contentType: "application/json",
                beforeSend: function (xhr) {
                    $.blockUI({ message: '<h4>Procesando...</h4>' });
                },
                success: function (data, status) {
                    if (status == "success") {
                        if (data.success) {
                            $.unblockUI();
                            $(".table").DataTable().ajax.reload();
                        }
                        else {
                            Mensajes.MostrarError(data.error);
                        }
                    }
                    else {
                        Mensajes.MostrarError("Se ha producido un error. Comuníquese con el administrador de ATSA.");
                        $.unblockUI();
                    }
                }
            });
        }, undefined);
    });
});