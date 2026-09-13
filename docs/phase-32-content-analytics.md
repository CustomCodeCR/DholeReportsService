# FASE 32 — Content analytics

`DholeReportsService` consume `dhole.reports.events` y persiste eventos de analítica de Content de forma idempotente por identificador de evento.

Eventos soportados:

- `content.analytics.page-viewed`
- `content.analytics.form-submitted`
- `content.analytics.interaction-clicked`
- `content.meeting.requested`

La tabla `reports.content_analytics_events` conserva dimensiones necesarias para páginas, campañas/UTM, formularios, meetings y CTAs/placements. Los endpoints `/api/reports/content-analytics/*` agregan visitas, campañas, conversiones, funnel y top interactions sin duplicar esta lógica en ContentService.
