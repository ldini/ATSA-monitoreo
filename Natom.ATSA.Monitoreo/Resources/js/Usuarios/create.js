$(document).ready(function () {
    $("form").validate({
        rules: {
            Nombre: {
                required: true,
                maxlength: 50
            },
            Apellido: {
                required: true,
                maxlength: 50
            },
            Email: {
                required: true,
                email: true,
                minlength: 5,
                maxlength: 100
            }
        },
        errorPlacement: function (error, element) {
            if (element.is("select")) {
                error.insertAfter("#" + element.attr("id") + "_chosen");
            } else {
                error.insertAfter(element);
            }
        }
    });
   
});

function Grabar() {
    if (!$("form").valid()) {
        return;
    }

    if ($("#Nombre").val().length == 0) {
        Mensajes.MostrarError("Debes ingresar el Nombre.");
        $("#Nombre").focus();
        return;
    }

    if ($("#Apellido").val().length == 0) {
        Mensajes.MostrarError("Debes ingresar el Apellido.");
        $("#Apellido").focus();
        return;
    }

    if ($("#Email").val().length == 0) {
        Mensajes.MostrarError("Debes ingresar el Email.");
        $("#Email").focus();
        return;
    }

    if ($(".chkPermiso:checked").length == 0) {
        Mensajes.MostrarError("Debes seleccionar al menos un permiso.");
        return;
    }

    var obj = {
        UsuarioId: parseInt($("#UsuarioId").val()),
        Nombre: $("#Nombre").val(),
        Apellido: $("#Apellido").val(),
        PuedeCargarExcelFacturas: $("#PuedeCargarExcelFacturas").is(":checked"),
        PuedeEliminarExcelFacturas: $("#PuedeEliminarExcelFacturas").is(":checked"),
        PuedeCargarExcelAuditoria: $("#PuedeCargarExcelAuditoria").is(":checked"),
        PuedeEliminarExcelAuditoria: $("#PuedeEliminarExcelAuditoria").is(":checked"),
        PuedeAuditarFactura: $("#PuedeAuditarFactura").is(":checked"),
        PuedeAuditarConsiliacion: $("#PuedeAuditarConsiliacion").is(":checked"),
        PuedeVerAuditorias: $("#PuedeVerAuditorias").is(":checked"),
        PuedeDescargarExcelAuditoria: $("#PuedeDescargarExcelAuditoria").is(":checked"),
        PuedeDarDeAltaPeriodo: $("#PuedeDarDeAltaPeriodo").is(":checked"),
        PuedeDarDeBajaPeriodo: $("#PuedeDarDeBajaPeriodo").is(":checked"),
        PuedeCargarEnPeriodo: $("#PuedeCargarEnPeriodo").is(":checked"),
        PuedeAnularEnPeriodo: $("#PuedeAnularEnPeriodo").is(":checked"),
        Email: $("#Email").val()
    };
    
    $.ajax({
        type: "POST",
        url: '/monitoreotest/usuarios/Grabar',
        data: JSON.stringify({
            usuario: obj
        }),
        dataType: "json",
        contentType: "application/json",
        beforeSend: function (xhr) {
            $.blockUI({ message: '<h4>Procesando...</h4>' });
        },
        success: function (data, status) {
            if (status == "success") {
                if (data.success) {
                    location.href = "/monitoreotest/Usuarios";
                }
                else {
                    Mensajes.MostrarError(data.error);
                    $.unblockUI();
                }
            }
            else {
                Mensajes.MostrarError("Se ha producido un error. Comuníquese con el administrador de ATSA.");
                $.unblockUI();
            }
        }
    });
    
}