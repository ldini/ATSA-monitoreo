CREATE DATABASE monitoreo;
USE monitoreo;

CREATE TABLE Usuario
(
	UsuarioId INT NOT NULL AUTO_INCREMENT,
	Nombre NVARCHAR(50) NOT NULL,
    Apellido NVARCHAR(50) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    Clave CHAR(32),
    
    FechaHoraAlta DATETIME NOT NULL,
    FechaHoraBaja DATETIME,
    Token CHAR(32),
    
    PuedeCargarExcelFacturas BIT NOT NULL,
    PuedeEliminarExcelFacturas BIT NOT NULL,
    
    PuedeCargarExcelAuditoria BIT NOT NULL,
    PuedeEliminarExcelAuditoria BIT NOT NULL,
    
    PuedeAuditarFactura BIT NOT NULL,
    PuedeAuditarConsiliacion BIT NOT NULL,
    
    PuedeVerAuditorias BIT NOT NULL,
    PuedeDescargarExcelAuditoria BIT NOT NULL,
    
    PRIMARY KEY (UsuarioId)
);

CREATE TABLE Prestacion
(
	PrestacionId INT NOT NULL AUTO_INCREMENT,
    Codigo NVARCHAR(10) NOT NULL,
    Descripcion NVARCHAR(100),
    PRIMARY KEY (PrestacionId)
);
INSERT INTO Prestacion (Codigo, Descripcion) VALUES ('QX', 'Cirugía');
INSERT INTO Prestacion (Codigo, Descripcion) VALUES ('CL', 'Clínica');
INSERT INTO Prestacion (Codigo, Descripcion) VALUES ('P', 'Pediatría');
INSERT INTO Prestacion (Codigo, Descripcion) VALUES ('NEO', 'Neonatología');
INSERT INTO Prestacion (Codigo, Descripcion) VALUES ('OB', 'Obstetricia');


CREATE UNIQUE INDEX indPrestacionCodigo ON Prestacion(Codigo);

CREATE TABLE Clinica
(
	ClinicaId INT NOT NULL AUTO_INCREMENT,
    Descripcion NVARCHAR(200),
    PRIMARY KEY (ClinicaId)
);

CREATE TABLE Carga
(
	CargaId INT NOT NULL AUTO_INCREMENT,
    Numero INT NOT NULL,
    
    Mes INT NOT NULL,
    Anio INT NOT NULL,
    
    ClinicaId INT NOT NULL,
    
    NombreArchivoExcel NVARCHAR(100),
    CargadoFechaHora DATETIME NOT NULL,
    CargadoPorUsuarioId INT,
    AnuladoFechaHora DATETIME,
    AnuladoMotivo NVARCHAR(200),
    AnuladoPorUsuarioId INT,
    
    PRIMARY KEY (CargaId),
    FOREIGN KEY (ClinicaId) REFERENCES Clinica(ClinicaId)
);

ALTER TABLE Carga ADD DescargaExcelConsiliacionFechaHora DATETIME;
ALTER TABLE Carga ADD DescargaExcelUsuarioId INT;
ALTER TABLE Carga ADD CargaExcelConsiliacionFechaHora DATETIME;
ALTER TABLE Carga ADD CargaExcelUsuarioId INT;

CREATE TABLE Factura
(
	FacturaId INT NOT NULL AUTO_INCREMENT,
    CargaId INT NOT NULL,
    NOrden NVARCHAR(100),
    XX NVARCHAR(50),
    IT NVARCHAR(50),
    Afiliado NVARCHAR(50),
    ApellidoYNombre NVARCHAR(150),
    
    Edad INT NOT NULL, #ESTE DATO SALDRÁ CALCULADO AL MOMENTO DE HACER LA CARGA BUSCANDO EL AFILIADO EN LA DB DE PRODUCCIÓN DE ATSA
    
    Practica NVARCHAR(50),
    Can NVARCHAR(50),
    Prestacion NVARCHAR(150),
    PrestacionId INT,
    Honor DECIMAL(18,2),
    Gastos DECIMAL(18,2),
    Fecha DATETIME,
    Filial NVARCHAR(50),
    Cabecera NVARCHAR(50),
    Detalle NVARCHAR(50),
    ErrorCab NVARCHAR(50),
    ErrorDet nvarchar(50),
    
    AuditoriaAprobado BIT,
    AuditoriaFechaHora DATETIME,
    AuditoriaAuditorFundamento NVARCHAR(500),
    AuditoriaAuditorMonto DECIMAL(18,2),
    AuditoriaUsuarioId INT,
    
    ConsiliacionPrestadorOferta DECIMAL(18,2),
    ConsiliacionPrestadorFundamento NVARCHAR(500),
    ConsiliacionAuditorAprueba BIT,
    ConsiliacionFechaHora DATETIME,
    ConsiliacionAuditorFundamento NVARCHAR(500),
    ConsiliacionUsuarioId INT,
    
    PRIMARY KEY (FacturaId),
    FOREIGN KEY (CargaId) REFERENCES Carga(CargaId),
    FOREIGN KEY (PrestacionId) REFERENCES Prestacion(PrestacionId)
);

ALTER TABLE Factura ADD NInterno NVARCHAR(50);
ALTER TABLE Factura MODIFY COLUMN AuditoriaFechaHora DATETIME;
ALTER TABLE Factura MODIFY COLUMN ConsiliacionFechaHora DATETIME;

DELIMITER $$
DROP PROCEDURE IF EXISTS `ListarCargas` $$
CREATE PROCEDURE `ListarCargas`(
	pClinicaId INT
)
BEGIN

	SELECT
		CA.CargaId,
        CA.CargadoFechaHora,
		CA.Numero,
		CA.Mes,
		CA.Anio,
		CLI.ClinicaId,
		CLI.Descripcion AS Clinica,
		SUM(1) AS Facturas,
        SUM(CASE WHEN FA.AuditoriaAprobado = 0 THEN 1 ELSE 0 END) AS FacturasRechazadasEnAuditoria,
		SUM(CASE WHEN FA.AuditoriaFechaHora IS NULL THEN 1 ELSE 0 END) AS PendienteAuditar,
		SUM(CASE WHEN FA.AuditoriaAprobado = 0 AND FA.ConsiliacionFechaHora IS NULL THEN 1 ELSE 0 END) AS PendienteConsiliar,
		
        CASE WHEN (CA.AnuladoFechaHora IS NOT NULL) THEN
			'Anulado'
		WHEN SUM(CASE WHEN FA.AuditoriaFechaHora IS NULL THEN 1 ELSE 0 END) /* PendienteAuditar */ = SUM(1) /* Facturas */ THEN
			'Pendiente de auditar'
        WHEN SUM(CASE WHEN FA.AuditoriaFechaHora IS NULL THEN 1 ELSE 0 END) /* PendienteAuditar */ > 0 THEN
			'Auditoría'
		WHEN SUM(CASE WHEN FA.AuditoriaAprobado = 0 THEN 1 ELSE 0 END) /*TotalAConsiliar*/ > 0 AND SUM(CASE WHEN FA.AuditoriaAprobado = 0 AND FA.ConsiliacionFechaHora IS NULL THEN 1 ELSE 0 END) /* PendienteConsiliar */ = SUM(CASE WHEN FA.AuditoriaAprobado = 0 THEN 1 ELSE 0 END) /* ParaConsiliar*/ THEN
			'Pendiente de consiliación'
        WHEN SUM(CASE WHEN FA.AuditoriaAprobado = 0 AND FA.ConsiliacionFechaHora IS NULL THEN 1 ELSE 0 END) /* PendienteConsiliar */ > 0 THEN
			'Consiliación'
		ELSE
			'Completado'
		END AS Estado,
        
        CASE WHEN (CA.AnuladoFechaHora IS NULL) THEN FALSE ELSE TRUE END AS Anulado
	FROM
		Carga CA
		INNER JOIN Clinica CLI ON CLI.ClinicaId = CA.ClinicaId
		INNER JOIN Factura FA ON FA.CargaId = CA.CargaId
	WHERE
		CA.ClinicaId = COALESCE(pClinicaId, CA.ClinicaId)
	GROUP BY
		CA.CargaId,
		CA.Numero,
		CA.Mes,
		CA.Anio,
		CLI.ClinicaId,
		CLI.Descripcion;
        
END $$
DELIMITER ;



DELIMITER $$
DROP PROCEDURE IF EXISTS `ListarAuditorias` $$
CREATE PROCEDURE `ListarAuditorias`(
	pClinicaId INT
)
BEGIN

	SELECT
		CA.CargaId,
        CA.CargadoFechaHora,
		CA.Numero,
		CA.Mes,
		CA.Anio,
		CLI.ClinicaId,
		CLI.Descripcion AS Clinica,
		SUM(1) AS Facturas,
        SUM(CASE WHEN FA.AuditoriaAprobado = 0 THEN 1 ELSE 0 END) AS FacturasRechazadasEnAuditoria,
		SUM(CASE WHEN FA.AuditoriaFechaHora IS NULL THEN 1 ELSE 0 END) AS PendienteAuditar,
		SUM(CASE WHEN FA.AuditoriaAprobado = 0 AND FA.ConsiliacionFechaHora IS NULL THEN 1 ELSE 0 END) AS PendienteConsiliar,
		
        CASE WHEN SUM(CASE WHEN FA.AuditoriaFechaHora IS NULL THEN 1 ELSE 0 END) /* PendienteAuditar */ = SUM(1) /* Facturas */ THEN
			'Pendiente de auditar'
        WHEN SUM(CASE WHEN FA.AuditoriaFechaHora IS NULL THEN 1 ELSE 0 END) /* PendienteAuditar */ > 0 THEN
			'Auditoría'
		WHEN SUM(CASE WHEN FA.AuditoriaAprobado = 0 THEN 1 ELSE 0 END) /*TotalAConsiliar*/ > 0 AND SUM(CASE WHEN FA.AuditoriaAprobado = 0 AND FA.ConsiliacionFechaHora IS NULL THEN 1 ELSE 0 END) /* PendienteConsiliar */ = SUM(CASE WHEN FA.AuditoriaAprobado = 0 THEN 1 ELSE 0 END) /* ParaConsiliar*/ THEN
			'Pendiente de consiliación'
        WHEN SUM(CASE WHEN FA.AuditoriaAprobado = 0 AND FA.ConsiliacionFechaHora IS NULL THEN 1 ELSE 0 END) /* PendienteConsiliar */ > 0 THEN
			'Consiliación'
		ELSE
			'Completado'
		END AS Estado,
        
        CASE WHEN CA.DescargaExcelConsiliacionFechaHora IS NULL THEN FALSE ELSE TRUE END AS ExcelDescargado,
        CASE WHEN CA.CargaExcelConsiliacionFechaHora IS NULL THEN FALSE ELSE TRUE END AS ExcelCargado
	FROM
		Carga CA
		INNER JOIN Clinica CLI ON CLI.ClinicaId = CA.ClinicaId
		INNER JOIN Factura FA ON FA.CargaId = CA.CargaId
	WHERE
		CA.AnuladoFechaHora IS NULL
		AND CA.ClinicaId = COALESCE(pClinicaId, CA.ClinicaId)
	GROUP BY
		CA.CargaId,
		CA.Numero,
		CA.Mes,
		CA.Anio,
		CLI.ClinicaId,
		CLI.Descripcion;
        
END $$
DELIMITER ;


DELIMITER $$
DROP PROCEDURE IF EXISTS `ListarDeudasAlPeriodo` $$
CREATE PROCEDURE `ListarDeudasAlPeriodo`(
	pAlPeriodo INTEGER
)
BEGIN

	SELECT
		F.NOrden AS NroFactura,
		CL.ClinicaId,
		CL.Descripcion AS RazonSocial,
		'' AS CUIT,
		CONCAT(LPAD(CA.Mes, 2, '0'), '-', CA.Anio) AS Periodo,
		F.Honor + F.Gastos AS ImporteFactura,
        F.Fecha AS FechaFactura,
		CAST(CONCAT(CA.Anio, LPAD(CA.Mes, 2, '0')) AS UNSIGNED INTEGER) AS PeriodoFiltro
	FROM
		Factura F
		INNER JOIN Carga CA ON CA.CargaId = F.CargaId
		INNER JOIN Clinica CL ON CL.ClinicaId = CA.ClinicaId
	WHERE
		F.ConsiliacionAuditorAprueba = 0
		AND CA.AnuladoFechaHora IS NULL
	HAVING
		PeriodoFiltro <= pAlPeriodo
	ORDER BY
		F.Fecha ASC;
        
END $$
DELIMITER ;

ALTER TABLE Factura ADD AuditoriaNumero NVARCHAR(50);
ALTER TABLE Factura ADD AuditoriaIngreso NVARCHAR(50);
ALTER TABLE Factura ADD AuditoriaEgreso NVARCHAR(50);
ALTER TABLE Factura ADD AuditoriaDebito NVARCHAR(50);



/********* UPDATE 13/01/2020 **********/
CREATE TABLE TipoComprobante
(
	TipoComprobanteId INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
    Codigo NVARCHAR(10),
    Descripcion NVARCHAR(50),
    Signo INT
);

INSERT INTO TipoComprobante(Codigo, Descripcion, Signo) VALUES
			('FC-A', 'Factura A', 1),
            ('FC-B', 'Factura B', 1),
            ('FC-C', 'Factura C', 1),
            ('NC-A', 'Nota de Crédito A', -1),
            ('NC-B', 'Nota de Crédito B', -1),
            ('NC-C', 'Nota de Crédito C', -1),
            ('ND-A', 'Nota de Débito A', 1),
            ('ND-B', 'Nota de Débito B', 1),
            ('ND-C', 'Nota de Débito C', 1);
INSERT INTO TipoComprobante(Codigo, Descripcion, Signo) VALUES
			('RBO', 'Recibo', 1);

CREATE TABLE Prestador
(
	PrestadorId INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
    RazonSocial NVARCHAR(200),
    CUIT NVARCHAR(20)
);

INSERT INTO Prestador (RazonSocial, CUIT) VALUES
				('AAARBA-ASOC.DE ANEST.ANAL.Y REHA.DE BS.AS.', '30-58699951-2'),
				('ALTERGARTEN S.A', '30-64233611-4'),
				('AISTENCIA INTEGRAL SANISOL', '30-70989129-0'),
				('BASA-UTE', '30-707959106'),
				('CENTRO DE DIAGNOSTICO Y TRAT. JONAS SALK', '30-69054402-0'),
				('CENTRO DE REHABILITACION SAN JUAN DE DIOS', '30-69648290-6'),
                ('CEPRO (FKT)', '30-70831007-3'),
				('CITAP', '30-68307286-5'),
                ('CLINICA DE MICROCIRUGIA', '33-54592544-9'),
				('CL.GRAL.OBS.Y CIR.NSTRA.SRA.DE FATIMA S.A.', '33-55483791-9'),
                ('CL.PRIV.DRES. TACHELLA S.A.', '30-58183537-6'),
				('CLINICA CRUZ CELESTE S.A.C. Y M.', '30-54615993-7'),
                ('CLINICA NUEVA CRUZ AZUL', '30-71418240-0'),
				('CEGEMED', '30-71167782-4'),
                ('CLINICA GÜEMES S.A.', '30-54606261-5'),
				('CLINICA PRIVADA ALCORTA S.A', '30-61565164-4'),
                ('CLINICA PRIVADA CENTRO', '30-56878488-6'),
				('CLINICA PRIVADA DEL BUEN PASTOR', '30-61534960-3'),
                ('CLINICA PRIVADA DEL CARMEN S.R.L.', '30-56222759-4'),
				('CLINICA PRIVADA PROVINCIAL S.A.', '33-60730840-9'),
                ('CLINICA SANTA MARIA - OSTEON S.A.', '30-62829148-5'),
				('CLINICA Y MATERNIDAD SAN JOSE OBRERO', '30-50225129-1'),
                ('CTRO. DE CIRUGIA LASER (ORL)', '30-70770613-5'),
				('DIAGNOSTICO BIOQUIMICO DEL OESTE S.R.L', '30-69707610-3'),
				('DIABVERUM ARGENTINA', '30-69081505-9'),
                ('DR AUCHE', '27-13020375-8'),
				('DR.FEDERICO JORGE ISRAEL(OFTALMOLOGO)', '20-21483350-7');


INSERT INTO Prestador (RazonSocial, CUIT) VALUES
				('DR. LOVISOLO, RUBEN (Cx Ginecologica)', '20-04559837-4'),
				('DR.GARCIA, VICTOR OSCAR (Cirugías O y T)', '20-22350310-2'),
				('DR.OTEGUI CALZADA, LUIS PABLO (Cirugías O y T)', '20-24524746-0'),
				('DR. KUYNDJIAN HUGO (Cx Ginecologica)', '20-16642218-4'),
				('DRA. AQUINO, GABRIELA (Cx O y T Pediatrica)', '27-25422627-6'),
				('DR. BENITEZ, JUAN ANDRES (Cirugías O y T)', '20-24792344-7'),
				('DRA. RUSSO MARIA ANUNCIACION (Cx Ginecologica)', '27-11321489-4'),
				('DRA. CAHAUD SORAYA ANABELLA (Cx Ginecologica)', '27-23454281-3'),
				('DRA. BREDEN XIMENA FERNANDA (Cx Pediatrica)', '27-26625466-6'),
                ('DR. HANSEN, MARTIN ALEJANDRO (Cirugia Gral.)', '20-17107115-2'),
				('DRA. SCARAMAL, LILIANA (Cirugías Gral.)', '27-10674985-5'),
				('DR, TURCO', '20-08344047-4'),
				('DOMICILIO SALUD SRL', '30-70976682-8'),
				('Dra. Lencioni (anatomia patologica)', '27-16218413-5'),
				('DR VASQUEZ OSINAGA SILFREDO', '20-18842809-7'),
				('FRESENIUS MEDICAL CARE S.A', '30-63581520-1'),
                ('GARNICA HERRERA IRENE', '27-42092979-5'),
				('GLADYS E. SANCHEZ RIQUELME', '27-92307476-2'),
				('HOSPITAL DUHAU', '30-54585047-4'),
				('HOSPITAL PRIV.LA MERCED', '30-56888885-1'),
				('I.A.M.I (Instituto de Atencion Medica Integral)', '30-70731228-5'),
				('IMAC S.R.L.', '30-68066417-6'),
				('INSTITUTO MEDICO CONSTITUYENTES S.A.', '30-58915037-2'),
				('INSTITUTOS MEDICOS S.A (Clínica Modelo Morón)', '30-54584921-2'),
				('MARIN', '27-25732460-0'),
				('MEDICINA ASIST. DE SERVICIOS S.A. (MAS)', '30-70807973-8'),
				('MEDICINA CATAN', '30707758267'),
				('MEGADIAGNOSTICO S.A.', '30-69895054-0'),
				('MELIAN', '27-35056399-2'),
				('NEFROLOGIA ARGENTINA', '30-69638893-4'),
				('OPSA (ODONTOLOGIA PERSONALIZADA S.A.)', '30-64240248-6'),
				('ORTOPEDIA BERNAT', '30-66114361-0'),
				('PRO-MED', '23-14596842-9'),
				('PARARED S.A.', '30-71046430-4'),
				('PRO.SA.M (Salud Mental)', '30-65453210-5'),
				('SANAT.PRIV.FIGUEROA PAREDES (I.CASANOVA)', '30-62309998-5'),
				('SICOMED (Figueroa Paredes Laferrere)', '30-70758470-6'),
				('SANAT.PRIV.FIGUEROA PAREDES (MARIANO ACOSTA)', '30-69158296-1'),
				('SANATORIO AMTA', '30-52317976-0'),
				('SANATORIO MOD. DE CASEROS S.A.', '30-51938929-7'),
				('+ SALUD', '30-71089654-9'),
				('TRAINE', '20-14467007-9'),
				('URODIAGNOSTICO(Centro Urologico)', '23-21113621-9'),
				('ZENTRUM', '30-71013005-8');

CREATE TABLE ComprobanteRecibidoPeriodo
(
	ComprobanteRecibidoPeriodoId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Mes INT,
    Anio INT,
    CreaUsuarioId INT NOT NULL,
    CreaFechaHora DATETIME NOT NULL,
    AnulaUsuarioId INT,
    AnulaFechaHora DATETIME,
    AnulaMotivo NVARCHAR(200)
);

CREATE TABLE ComprobanteRecibido
(
	ComprobanteRecibidoId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    ComprobanteRecibidoPeriodoId INT NOT NULL,
    FechaRecibido DATE NOT NULL,
    PrestadorId INT NOT NULL,
    TipoComprobanteId INT NOT NULL,
    Fecha DATE NOT NULL,
    PuntoDeVenta INT,
    Numero INT,
    NInterno NVARCHAR(20),
    MesPeriodo INT,
    AnioPeriodo INT,
    Refact BIT(1),
    Monto DECIMAL(18,2),
    Debito DECIMAL(18,2),
    APagar DECIMAL(18,2), /* MONTO - DEBITO */
    TipoSoporteMag BIT(1),
    Ambul BIT(1),
    `Int` BIT(1),
    Observaciones NVARCHAR(200),
    
    CargaFechaHora DATETIME,
    CargaUsuarioId INT,
    
    Liquidado BIT(1),
    LiquidadoUsuarioId INT,
    
    Babich BIT(1),
    GT BIT(1),
    
    FOREIGN KEY (TipoComprobanteId) REFERENCES TipoComprobante(TipoComprobanteId),
    FOREIGN KEY (PrestadorId) REFERENCES Prestador(PrestadorId),
    FOREIGN KEY (ComprobanteRecibidoPeriodoId) REFERENCES ComprobanteRecibidoPeriodo(ComprobanteRecibidoPeriodoId)
);

ALTER TABLE ComprobanteRecibido ADD EliminadoFechaHora DATETIME;
ALTER TABLE ComprobanteRecibido ADD EliminadoUsuarioId INT;
ALTER TABLE ComprobanteRecibido ADD EliminadoMotivo NVARCHAR(200);


/******** UPDATE 15/01/2020 **********/
DELIMITER $$
DROP PROCEDURE IF EXISTS `ListarComprobantePeriodo` $$
CREATE PROCEDURE `ListarComprobantePeriodo`(

)
BEGIN

	SELECT
		CRP.ComprobanteRecibidoPeriodoId AS Id,
		CRP.Mes,
        CRP.Anio,
        SUM(CASE WHEN CR.ComprobanteRecibidoId IS NULL THEN 0 ELSE 1 END) AS TotalComprobantes,
        SUM((CR.Monto - CR.Debito) * Signo) AS SumaTotal,
        CASE WHEN CRP.AnulaUsuarioId IS NULL THEN FALSE ELSE TRUE END AS Anulado
	FROM
		ComprobanteRecibidoPeriodo CRP
        LEFT JOIN ComprobanteRecibido CR ON CRP.ComprobanteRecibidoPeriodoId = CR.ComprobanteRecibidoPeriodoId AND CR.EliminadoUsuarioId IS NULL
        LEFT JOIN TipoComprobante T ON T.TipoComprobanteId = CR.TipoComprobanteId
	WHERE
		CRP.ComprobanteRecibidoPeriodoId IS NOT NULL
	GROUP BY
		CRP.ComprobanteRecibidoPeriodoId,
		CRP.Mes,
        CRP.Anio,
        CRP.AnulaUsuarioId;

END $$
DELIMITER ;


ALTER TABLE Usuario ADD PuedeDarDeAltaPeriodo BIT(1) NOT NULL;
ALTER TABLE Usuario ADD PuedeDarDeBajaPeriodo BIT(1) NOT NULL;
ALTER TABLE Usuario ADD PuedeCargarEnPeriodo BIT(1) NOT NULL;
ALTER TABLE Usuario ADD PuedeAnularEnPeriodo BIT(1) NOT NULL;

ALTER TABLE ComprobanteRecibido ADD LiquidadoPor NVARCHAR(100);


/******** UPDATE 26/01/2020 ***********/
ALTER TABLE ComprobanteRecibido ADD MontoInt DECIMAL(18,2);

/******** UPDATE 02/02/2020 ***********/
ALTER TABLE ComprobanteRecibido MODIFY COLUMN Numero NVARCHAR(50);
ALTER TABLE ComprobanteRecibido ADD Consiliado BIT(1) DEFAULT 0;

DELIMITER $$
DROP PROCEDURE IF EXISTS `ListarComprobantePeriodo` $$
CREATE PROCEDURE `ListarComprobantePeriodo`(
	pUsuarioId INT
)
BEGIN

	SELECT
		CRP.ComprobanteRecibidoPeriodoId AS Id,
		CRP.Mes,
        CRP.Anio,
        SUM(CASE WHEN CR.ComprobanteRecibidoId IS NULL THEN 0 ELSE 1 END) AS TotalComprobantes,
        SUM((CR.Monto - CR.Debito) * Signo) AS SumaTotal,
        CASE WHEN CRP.AnulaUsuarioId IS NULL THEN FALSE ELSE TRUE END AS Anulado,
        COALESCE(CONCAT(U.Nombre, ' ', U.Apellido), 'admin') AS ResponsableNombreYApellido,
        CRP.CreaUsuarioId AS ResponsableUsuarioId
	FROM
		ComprobanteRecibidoPeriodo CRP
        LEFT JOIN Usuario U ON U.UsuarioId = CRP.CreaUsuarioId
        LEFT JOIN ComprobanteRecibido CR ON CRP.ComprobanteRecibidoPeriodoId = CR.ComprobanteRecibidoPeriodoId AND CR.EliminadoUsuarioId IS NULL
        LEFT JOIN TipoComprobante T ON T.TipoComprobanteId = CR.TipoComprobanteId
	WHERE
		CRP.ComprobanteRecibidoPeriodoId IS NOT NULL
        AND (pUsuarioId = 0 OR CRP.CreaUsuarioId = pUsuarioId)
	GROUP BY
		CRP.ComprobanteRecibidoPeriodoId,
		CRP.Mes,
        CRP.Anio,
        CRP.AnulaUsuarioId;

END $$
DELIMITER ;

/******** UPDATE 04/02/2020 ***********/
ALTER TABLE ComprobanteRecibido ADD MT BIT(1);

DELIMITER $$
DROP PROCEDURE IF EXISTS `ListarDeudasAlPeriodo` $$
CREATE PROCEDURE `ListarDeudasAlPeriodo`(
	pAlPeriodo INTEGER
)
BEGIN

	SELECT
		CONCAT(T.Codigo, ' ', C.Numero) AS NroFactura,
		P.PrestadorId AS ClinicaId,
		P.RazonSocial AS RazonSocial,
		P.CUIT AS CUIT,
		CONCAT(LPAD(C.MesPeriodo, 2, '0'), '-', C.AnioPeriodo) AS Periodo,
		C.APagar AS ImporteFactura,
		C.Fecha AS FechaFactura,
		CAST(CONCAT(C.AnioPeriodo, LPAD(C.MesPeriodo, 2, '0')) AS UNSIGNED INTEGER) AS PeriodoFiltro
	FROM
		comprobanterecibidoperiodo CA
		INNER JOIN ComprobanteRecibido C ON C.ComprobanteRecibidoPeriodoId = CA.ComprobanteRecibidoPeriodoId
		INNER JOIN TipoComprobante T ON T.TipoComprobanteId = C.TipoComprobanteId
		INNER JOIN Prestador P ON P.PrestadorId = C.PrestadorId
	WHERE
		CA.AnulaFechaHora IS NULL
		AND C.Liquidado = 0
	HAVING
		PeriodoFiltro <= pAlPeriodo
	ORDER BY
		C.Fecha ASC;
        
END $$
DELIMITER ;


/********* UPDATE 16/02/2020 ***********/
ALTER TABLE ComprobanteRecibido ADD Tipo CHAR(1);
ALTER TABLE ComprobanteRecibido ADD FechaInformada DATE;

/********* UPDATE 03/03/2020 **********/
INSERT INTO TipoComprobante(Codigo, Descripcion, Signo) VALUES
			('RBO-B', 'Recibo B', 1);
INSERT INTO TipoComprobante(Codigo, Descripcion, Signo) VALUES
			('RBO-C', 'Recibo C', 1);
            
            
/******** UPDATE 01/04/2020 **********/
ALTER TABLE ComprobanteRecibido ADD Pago1 DECIMAL(18,2);
ALTER TABLE ComprobanteRecibido ADD Pago2 DECIMAL(18,2);
ALTER TABLE ComprobanteRecibido ADD Pago3 DECIMAL(18,2);
ALTER TABLE ComprobanteRecibido ADD PagoObservaciones NVARCHAR(200);

DELIMITER $$
DROP PROCEDURE IF EXISTS `ListarFacturasConSaldoParaPagar` $$
CREATE PROCEDURE `ListarFacturasConSaldoParaPagar`(
	pTipoComprobanteId INT,
    pPrestadorId INT,
    pMesPeriodo INT,
    pAnioPeriodo INT
)
BEGIN

	SELECT
		CR.ComprobanteRecibidoId,
        CRP.ComprobanteRecibidoPeriodoId,
        P.RazonSocial AS Prestador,
        P.CUIT AS PrestadorCUIT,
        CONCAT(T.Codigo, ' ', CR.Numero) AS Comprobante,
        CR.Fecha,
        CONCAT(LPAD(CRP.Mes, 2, '0'), '-', CRP.Anio) AS Periodo,
        COALESCE(CONCAT(U.Nombre, ' ', U.Apellido), 'admin') AS Por,
        (CR.Monto - CR.Debito) AS Total,
        (CR.Monto - CR.Debito) - (COALESCE(CR.Pago1, 0) + COALESCE(CR.Pago2, 0) + COALESCE(CR.Pago3, 0)) AS Saldo,
        CR.Pago1,
        CR.Pago2,
        CR.Pago3,
        CR.PagoObservaciones
	FROM
		ComprobanteRecibido CR
        INNER JOIN ComprobanteRecibidoPeriodo CRP ON CRP.ComprobanteRecibidoPeriodoId = CR.ComprobanteRecibidoPeriodoId
        INNER JOIN Prestador P ON P.PrestadorId = CR.PrestadorId
        LEFT JOIN Usuario U ON U.UsuarioId = CRP.CreaUsuarioId
        LEFT JOIN TipoComprobante T ON T.TipoComprobanteId = CR.TipoComprobanteId
	WHERE
		CR.EliminadoUsuarioId IS NULL
		AND CRP.AnulaFechaHora IS NULL
        AND CR.TipoComprobanteId = COALESCE(pTipoComprobanteId, CR.TipoComprobanteId)
        AND CR.PrestadorId = COALESCE(pPrestadorId, CR.PrestadorId)
        AND CRP.Mes = COALESCE(pMesPeriodo, CRP.Mes)
        AND CRP.Anio = COALESCE(pAnioPeriodo, CRP.Anio);
        
END $$
DELIMITER ;

DELIMITER $$
DROP PROCEDURE IF EXISTS `ListarDeudasAlPeriodo` $$
CREATE PROCEDURE `ListarDeudasAlPeriodo`(
	pAlPeriodo INTEGER
)
BEGIN

	SELECT
		CONCAT(T.Codigo, ' ', C.Numero) AS NroFactura,
		P.PrestadorId AS ClinicaId,
		P.RazonSocial AS RazonSocial,
		P.CUIT AS CUIT,
		CONCAT(LPAD(C.MesPeriodo, 2, '0'), '-', C.AnioPeriodo) AS Periodo,
		(C.Monto - C.Debito) AS ImporteFactura,
        (C.Monto - C.Debito) - (COALESCE(C.Pago1, 0) + COALESCE(C.Pago2, 0) + COALESCE(C.Pago3, 0)) AS SaldoFactura,
		C.Fecha AS FechaFactura,
		CAST(CONCAT(C.AnioPeriodo, LPAD(C.MesPeriodo, 2, '0')) AS UNSIGNED INTEGER) AS PeriodoFiltro,
        CASE WHEN C.LiquidadoPor = 'ATSA' THEN TRUE ELSE FALSE END AS CargadoPorAdmin
	FROM
		comprobanterecibidoperiodo CA
		INNER JOIN ComprobanteRecibido C ON C.ComprobanteRecibidoPeriodoId = CA.ComprobanteRecibidoPeriodoId
		INNER JOIN TipoComprobante T ON T.TipoComprobanteId = C.TipoComprobanteId
		INNER JOIN Prestador P ON P.PrestadorId = C.PrestadorId
	WHERE
		CA.AnulaFechaHora IS NULL
        AND (COALESCE(C.Pago1, 0) + COALESCE(C.Pago2, 0) + COALESCE(C.Pago3, 0)) < (C.Monto - C.Debito)
	HAVING
		PeriodoFiltro <= pAlPeriodo
	ORDER BY
		C.Fecha ASC;
        
END $$
DELIMITER ;


/********** UPDATE 13/04/2020 ************/
DELIMITER $$
DROP PROCEDURE IF EXISTS `ListarFacturasConSaldoParaPagar` $$
CREATE PROCEDURE `ListarFacturasConSaldoParaPagar`(
	pTipoComprobanteId INT,
    pPrestadorId INT,
    pMesPeriodo INT,
    pAnioPeriodo INT
)
BEGIN

	SELECT
		CR.PrestadorId,
		CR.ComprobanteRecibidoId,
        CRP.ComprobanteRecibidoPeriodoId,
        P.RazonSocial AS Prestador,
        P.CUIT AS PrestadorCUIT,
        CONCAT(T.Codigo, ' ', CR.Numero) AS Comprobante,
        CR.Fecha,
        CONCAT(LPAD(CRP.Mes, 2, '0'), '-', CRP.Anio) AS Periodo,
        COALESCE(CONCAT(U.Nombre, ' ', U.Apellido), 'admin') AS Por,
        (CR.Monto - CR.Debito) AS Total,
        (CR.Monto - CR.Debito) - (COALESCE(CR.Pago1, 0) + COALESCE(CR.Pago2, 0) + COALESCE(CR.Pago3, 0)) AS Saldo,
        CR.Pago1,
        CR.Pago2,
        CR.Pago3,
        CR.PagoObservaciones
	FROM
		ComprobanteRecibido CR
        INNER JOIN ComprobanteRecibidoPeriodo CRP ON CRP.ComprobanteRecibidoPeriodoId = CR.ComprobanteRecibidoPeriodoId
        INNER JOIN Prestador P ON P.PrestadorId = CR.PrestadorId
        LEFT JOIN Usuario U ON U.UsuarioId = CRP.CreaUsuarioId
        LEFT JOIN TipoComprobante T ON T.TipoComprobanteId = CR.TipoComprobanteId
	WHERE
		CR.EliminadoUsuarioId IS NULL
		AND CRP.AnulaFechaHora IS NULL
        AND CR.TipoComprobanteId = COALESCE(pTipoComprobanteId, CR.TipoComprobanteId)
        AND CR.PrestadorId = COALESCE(pPrestadorId, CR.PrestadorId)
        AND CRP.Mes = COALESCE(pMesPeriodo, CRP.Mes)
        AND CRP.Anio = COALESCE(pAnioPeriodo, CRP.Anio);
        
END $$
DELIMITER ;

DELIMITER $$
DROP PROCEDURE IF EXISTS `ListarDeudasAlPeriodo` $$
CREATE PROCEDURE `ListarDeudasAlPeriodo`(
	pAlPeriodo INTEGER
)
BEGIN

	SELECT
		CONCAT(T.Codigo, ' ', C.Numero) AS NroFactura,
		P.PrestadorId AS ClinicaId,
		P.RazonSocial AS RazonSocial,
		P.CUIT AS CUIT,
		CONCAT(LPAD(C.MesPeriodo, 2, '0'), '-', C.AnioPeriodo) AS Periodo,
		(C.Monto - C.Debito) AS ImporteFactura,
        (C.Monto - C.Debito) - (COALESCE(C.Pago1, 0) + COALESCE(C.Pago2, 0) + COALESCE(C.Pago3, 0)) AS SaldoFactura,
		C.Fecha AS FechaFactura,
		CAST(CONCAT(C.AnioPeriodo, LPAD(C.MesPeriodo, 2, '0')) AS UNSIGNED INTEGER) AS PeriodoFiltro,
        CASE WHEN C.LiquidadoPor = 'ATSA' THEN TRUE ELSE FALSE END AS CargadoPorAdmin
	FROM
		comprobanterecibidoperiodo CA
		INNER JOIN ComprobanteRecibido C ON C.ComprobanteRecibidoPeriodoId = CA.ComprobanteRecibidoPeriodoId
		INNER JOIN TipoComprobante T ON T.TipoComprobanteId = C.TipoComprobanteId
		INNER JOIN Prestador P ON P.PrestadorId = C.PrestadorId
	WHERE
		CA.AnulaFechaHora IS NULL
        AND ((C.Monto - C.Debito) - (COALESCE(C.Pago1, 0) + COALESCE(C.Pago2, 0) + COALESCE(C.Pago3, 0))) != 0 
	HAVING
		PeriodoFiltro <= pAlPeriodo
	ORDER BY
		C.Fecha ASC;
        
END $$
DELIMITER ;


/******** UPDATE 08/05/2020 *********/
ALTER TABLE ComprobanteRecibido ADD Auditado BIT(1) NOT NULL DEFAULT 0;

/******** UPDATE 08/12/2020 *********/
ALTER TABLE ComprobanteRecibido ADD InformadoEnPMIF BIT(1) NOT NULL DEFAULT 0;