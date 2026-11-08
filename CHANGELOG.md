# Changelog

## [1.5.2] - 2026-11-08 (F9 candidato, sin despliegue)

- CI principal de Reports ahora provee PostgreSQL 17 efímero y `REPORTS_F6_POSTGRES`, igual que la suite F6.09 ya validada. Se ejecuta el test transaccional real de plantilla y Outbox sin saltarlo ni modificar datos compartidos.
- Versionamiento PATCH 1.5.1 → 1.5.2; `ecosystemVersion=1.13.11` para el siguiente lote de candidato coordinado.


## [1.5.1] - 2026-11-08 (candidato F9, sin publicación)

- Sincroniza metadatos de versión global del batch Dhole Realtime `rt-20261107-01` a `1.13.10`; actualización PATCH independiente de DholeReportsService `1.5.0` → `1.5.1`. No cambia contratos REST, datos, lógica de negocio ni flags; requiere gates F0–F8 y CI de SHA final antes de merge.


## [1.5.0] - 2026-11-08 (candidato F6.09 no publicado)

- F6.09: los cambios de plantillas Reports generan invalidaciones mínimas de recursos en el Outbox transaccional PostgreSQL; migración EF aditiva con unicidad de EventId.
- Worker con reintentos publica a `dhole.reports.realtime.v1`, separado de generación PDF y flujos de analítica; no transmite HTML, diseños ni PDF. REST sigue autoritativo.
- Pruebas de seguridad y persistencia sobre PostgreSQL real, flags OFF por defecto. Componente 1.4.2 → 1.5.0, global candidato 1.13.0; sin deploy.

## [1.4.2] - 2026-11-07

- Rediseña el índice PDF con tarjetas por subQUO, rutas, naviera, Ocean Freight y enlaces al detalle; agrega validaciones HTML.
- Release DH-20261107-MULTIQUO-VALUE-INDEX-027. Merge coordinado a develop y master sujeto a CI y staging.

## [1.4.1] - 2026-11-07 (pendiente de CI/staging)

- Evita mostrar directivas de plantilla sin procesar en el PDF e integra el renderizador de cotización agrupada.
- Release DH-20261107-MULTIQUO-PERSISTED-PDF-026; sin autorización de despliegue a producción.

## [1.4.0] - 2026-11-06 (en desarrollo)

- Habilita el renderizador interno para un único documento PDF con índice navegable de sub-QUO, ruta, naviera y OF, manteniendo la plantilla HTML original de cada alternativa.
- Añade pruebas de enlaces, 30 opciones, escaping e integridad del documento; pendiente la integración completa con el grupo persistido de Pricing.
- Release coordinado DH-20261106-MULTI-QUO-018; no desplegado.


## [1.3.0] - 2026-11-06

- Muestra ruta y comentarios de la naviera, cuando se proporcionan, inmediatamente debajo del tiempo de tránsito en el PDF.
- Migra de forma idempotente las plantillas FCL y LCL activas almacenadas en PostgreSQL.
- Release DH-20261106-CARRIER-ROUTE-COMMENTS-017, Dhole 1.4.0.

## [1.2.1] - 2026-11-05

- Consolida SemVer por componente y despliegues automáticos a staging/producción por push.
- Sincroniza versión nativa y manifiesto del release global Dhole.

## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
