function GetUrl() {
    var filtro = $("#FiltroEstado").val();
    if (filtro.length > 0) {
        filtro = "?filtro=" + filtro;
    }
    return "/monitoreotest/auditorias/ObtenerListadoIndex" + filtro;
}

$(document).ready(function () {
    $("#Status").chosen();
    $("#Status").change(function () {
        var url = GetUrl();
        if (url.indexOf("?") >= 0) {
            url += "&status=" + $("#Status").val();
        }
        else {
            url += "?status=" + $("#Status").val();
        }
        $('#tbl').DataTable().ajax.url(url);
        $('#tbl').DataTable().ajax.reload();
    });

    if ($("#FiltroEstado").val().length == 0) {
        $("#divfiltros").show();
    }

    var oTable = $('#tbl').dataTable({
        "bProcessing": true,
        "bServerSide": true,
        "ajax": {
            "url": GetUrl(),
            "type": "POST"
        },
        "bLengthChange": true,
        "bFilter": true,
        "bSort": true,
        "bInfo": true,
        "order": [[6, "desc"]],
        "aoColumns": [
            { "bSortable": true, "sWidth": '14%' },
            { "bSortable": true, "sWidth": '9%' },
            { "bSortable": true, "sWidth": '8%' },
            { "bSortable": true, "sWidth": '24%' },
            { "bSortable": true, "sWidth": '9%' },
            { "bSortable": true, "sWidth": '15%' },
            {
                "bSortable": false, "sWidth": '21%',
                mRender: function (data, type, row) {
                    var html = "";
                    var puede = row[7];
                    var esAuditoria = row[8];
                    var esConsiliacion = row[9];
                    var bajoExcelConsiliacion = row[10];
                    var cargoExcelConsiliacion = row[11];
                    var esPendienteDeAuditoria = row[12];
                    var esPendienteDeConsiliacion = row[13];
                    var tienePermisoCargarExcel = row[14];
                    var tienePermisoDescargarExcel = row[15];
                    var tienePermisoAuditar = row[16];
                    var tienePermisoConsiliar = row[17];

                    if ((esAuditoria || esPendienteDeAuditoria) && tienePermisoAuditar) {
                        html += '&nbsp;<a class="btn btn-primary btn-xs btn-auditar" id="' + data + '">Ir a Auditar <span class="glyphicon glyphicon-chevron-right"></span></a>';
                    }
                    if ((esConsiliacion || esPendienteDeConsiliacion) && tienePermisoConsiliar) {
                        if (!cargoExcelConsiliacion && tienePermisoDescargarExcel) {
                            html += '&nbsp;<a class="btn btn-success btn-xs btn-bajar-excel" id="' + data + '" title="Bajar Excel consiliación"><span class="glyphicon glyphicon-download"></span> excel</a>';
                            if (bajoExcelConsiliacion && tienePermisoCargarExcel) {
                                html += '&nbsp;<a class="btn btn-warning btn-xs btn-subir-excel" id="' + data + '" title="Subir Excel consiliación"><span class="glyphicon glyphicon-upload"></span> excel</a>';
                            }
                        }
                        else {
                            html += '&nbsp;<a class="btn btn-warning btn-xs btn-consiliar" id="' + data + '">Ir a Consiliar <span class="glyphicon glyphicon-chevron-right"></span></a>';
                        }
                    }
                    
                    return html;
                }
            }
        ],

    });

    $("#tbl").on("click", ".btn-eliminar", function (e) {
        var id = e.currentTarget.id;
        var puede = $(this).is("[puede]");
        if (puede) {
            Mensajes.MostrarSiNo("¿Desea eliminar la carga?", function () {
                alert("Eliminar");
                $("#tbl").DataTable().ajax.url("/monitoreotest/cargas/ObtenerListadoIndex");
                $("#tbl").DataTable().ajax.reload();
            });
        }
        else {
            Mensajes.MostrarError("No puedes eliminar la carga porque ya se encuentra en auditoría.");
        }
    });

    $("#tbl").on("click", ".btn-auditar", function (e) {
        var id = e.currentTarget.id;
        location.href = "/monitoreotest/auditorias/VerAuditoria?cargaid=" + id;
    });

    $("#tbl").on("click", ".btn-consiliar", function (e) {
        var id = e.currentTarget.id;
        location.href = "/monitoreotest/auditorias/VerConsiliacion?cargaid=" + id;
    });

    $("#tbl").on("click", ".btn-bajar-excel", function (e) {
        var id = e.currentTarget.id;
        location.href = "/monitoreotest/auditorias/DownloadExcelAuditoria?cargaId=" + id;
        setTimeout(function () {
            $("#tbl").DataTable().ajax.reload();
        }, 4000);
    });

    $("#tbl").on("click", ".btn-subir-excel", function (e) {
        var id = e.currentTarget.id;
        CargarExcelAuditorias(id);
    });
});