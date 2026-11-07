# Changelog

## [1.4.0] - 2026-11-06 (en desarrollo)

- Habilita el renderizador interno para un único documento PDF con índice navegable de sub-QUO, ruta, naviera y OF, manteniendo la plantilla HTML original de cada alternativa.
- Añade pruebas de enlaces, 30 opciones, escaping e integridad del documento; pendiente la integración completa con el grupo persistido de Pricing.
- Release coordinado DH-20261106-MULTI-QUO-018; no desplegado.


## [1.3.0] - 2026-11-06

- Muestra ruta y comentarios de la naviera, cuando se proporcionan, inmediatamente debajo del tiempo de tránsito en el PDF.
- Migra de forma idempotente las plantillas FCL y LCL activas almacenadas en PostgreSQL.
- Release DH-20261106-CARRIER-ROUTE-COMMENTS-017, Dhole 1.4.0.

## [1.2.1] - 2026-11-05

- Actualiza versión SemVer del componente y metadatos del ecosistema Dhole 1.3.1.
- Garantiza despliegues por push en master/develop y preserva las comprobaciones CI.

## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
