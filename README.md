# Dhole Reports Service

Servicio de plantillas y generación documental construido con `CustomCodeFramework`:

- Clean Architecture por proyectos Domain, Application, Persistence, Infrastructure, API y Contracts.
- CQRS con `CustomCodeFramework.Cqrs`.
- resultados y errores con `CustomCodeFramework.Core`.
- PostgreSQL/EF Core con `CustomCodeFramework.Postgres.EntityFramework`.
- JWT y autorización por scopes con `CustomCodeFramework.Auth`.

## Funcionalidad

- CRUD de plantillas HTML y JSON del diseñador visual.
- Persistencia automática de una vista previa PDF al crear o modificar una plantilla.
- Variables HTML como `{{company.name}}`.
- Colecciones como `{{#each items}} ... {{description}} ... {{/each}}`.
- Generación de PDF desde HTML y datos JSON.
- Generación de XLSX y CSV desde arreglos JSON.
- Soft delete y nombre único entre plantillas activas/no eliminadas.

## Scopes

| Scope | Acción |
|---|---|
| `reports.templates.view` | listar, consultar y abrir el PDF almacenado |
| `reports.templates.create` | crear plantillas |
| `reports.templates.update` | modificar plantillas |
| `reports.templates.delete` | eliminar plantillas |
| `reports.reports.generate` | generar PDF, XLSX o CSV |

Los scopes se incluyen en el ZIP actualizado de `DholeAuthService`. El rol `SuperUser` los recibe automáticamente durante el seed; los demás usuarios/roles deben recibirlos desde Auth.

## Endpoints

- `GET /api/reports/templates`
- `GET /api/reports/templates/{templateId}`
- `GET /api/reports/templates/{templateId}/preview.pdf`
- `POST /api/reports/templates`
- `PUT /api/reports/templates/{templateId}`
- `DELETE /api/reports/templates/{templateId}`
- `POST /api/reports/templates/{templateId}/generate`

Ejemplo para generar:

```json
{
  "format": "pdf",
  "fileName": "tarifa-cliente",
  "sheetName": "Tarifas",
  "dataJson": "{\"company\":{\"name\":\"Dhole Logistics\"},\"items\":[{\"description\":\"Flete\",\"amount\":6300}]}"
}
```

## HTML a PDF

El orden de renderizado es:

1. Chromium headless.
2. WeasyPrint.
3. PDF básico de emergencia, si `AllowBasicPdfFallback` está habilitado.

Configuración:

```json
"Reports": {
  "Generation": {
    "ChromiumExecutablePath": null,
    "ChromiumTimeoutSeconds": 60,
    "WeasyPrintExecutablePath": null,
    "WeasyPrintTimeoutSeconds": 60,
    "AllowBasicPdfFallback": true
  }
}
```

En Arch/CachyOS puede instalar Chromium o WeasyPrint. Cuando la ruta queda en `null`, el servicio los busca en `PATH`.

## Ejecución

El servicio HTTP está configurado en `http://localhost:5208` y aplica automáticamente la migración `InitialReports` al iniciar.
