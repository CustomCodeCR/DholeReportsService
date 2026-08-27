using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260827234500_UpdatePricingFclClientQuoteTemplateLayout")]
public sealed class UpdatePricingFclClientQuoteTemplateLayout : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE reports.report_templates
            SET
                name = 'Cotización Pricing - Cliente',
                description = 'Plantilla oficial de cotización de Pricing para cliente.',
                html_content = $html$
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8" />

  <style>
    @page {
      size: A4;
      margin: 14mm 15mm 16mm;
    }

    * {
      box-sizing: border-box;
    }

    body {
      margin: 0;
      padding: 0;
      font-family: Arial, Helvetica, sans-serif;
      font-size: 11px;
      line-height: 1.45;
      color: #030202;
      background: #ffffff;
    }

    /* =========================================================
       HEADER
       ========================================================= */

    .header {
      width: 100%;
      margin-bottom: 18px;
      border-collapse: collapse;
    }

    .header td {
      border: 0;
      padding: 0;
      vertical-align: top;
    }

    .brand-area {
      width: 58%;
    }

    .quote-area {
      width: 42%;
      text-align: right;
    }

    .brand {
      margin-bottom: 3px;
      font-size: 24px;
      font-weight: 800;
      letter-spacing: -0.5px;
      color: #fc2800;
      text-transform: uppercase;
    }

    .brand-subtitle {
      font-size: 11px;
      font-weight: 700;
      color: #030202;
      text-transform: uppercase;
      letter-spacing: 1px;
    }

    .brand-line {
      width: 56px;
      height: 4px;
      margin: 8px 0 10px;
      background: #fc2800;
      border-radius: 2px;
    }

    .company-information {
      font-size: 10px;
      line-height: 1.55;
      color: #444444;
    }

    .quote-label {
      margin-bottom: 3px;
      font-size: 9px;
      font-weight: 700;
      letter-spacing: 1.2px;
      color: #666666;
      text-transform: uppercase;
    }

    .quote-number {
      margin-bottom: 8px;
      font-size: 21px;
      font-weight: 800;
      color: #fc2800;
    }

    .quote-meta {
      font-size: 10px;
      line-height: 1.7;
      color: #333333;
    }

    .header-divider {
      height: 4px;
      margin-bottom: 20px;
      background: #030202;
      border: 0;
      position: relative;
    }

    /* =========================================================
       TITLES
       ========================================================= */

    .section {
      margin-top: 20px;
      page-break-inside: avoid;
    }

    .section-title {
      margin: 0 0 10px;
      padding-bottom: 6px;
      font-size: 11px;
      font-weight: 800;
      letter-spacing: 0.8px;
      color: #fc2800;
      text-transform: uppercase;
      border-bottom: 2px solid #fc2800;
    }

    /* =========================================================
       ROUTE / SUMMARY
       ========================================================= */

    .route-card {
      margin-bottom: 17px;
      padding: 0;
      overflow: hidden;
      border: 1px solid #e5e5e5;
      border-radius: 8px;
      background: #ffffff;
    }

    .route-header {
      padding: 10px 13px;
      color: #ffffff;
      background: #030202;
    }

    .route-label {
      margin-bottom: 2px;
      font-size: 9px;
      font-weight: 700;
      letter-spacing: 0.8px;
      text-transform: uppercase;
      opacity: 0.8;
    }

    .route-value {
      font-size: 15px;
      font-weight: 700;
    }

    .route-details {
      width: 100%;
      border-collapse: collapse;
    }

    .route-details td {
      width: 33.333%;
      padding: 11px 13px;
      vertical-align: top;
      border: 0;
      border-right: 1px solid #ececec;
    }

    .route-details td:last-child {
      border-right: 0;
    }

    .data-label {
      display: block;
      margin-bottom: 3px;
      font-size: 8px;
      font-weight: 700;
      letter-spacing: 0.5px;
      color: #888888;
      text-transform: uppercase;
    }

    .data-value {
      font-size: 11px;
      font-weight: 700;
      color: #030202;
    }

    /* =========================================================
       GENERAL INFORMATION
       ========================================================= */

    .information-grid {
      width: 100%;
      margin-bottom: 16px;
      border-collapse: separate;
      border-spacing: 7px;
      margin-left: -7px;
      margin-right: -7px;
    }

    .information-grid td {
      width: 33.333%;
      padding: 9px 10px;
      vertical-align: top;
      border: 1px solid #e6e6e6;
      border-radius: 6px;
      background: #fafafa;
    }

    /* =========================================================
       CONTAINERS
       ========================================================= */

    .equipment-table {
      width: 100%;
      margin-top: 5px;
      border-collapse: collapse;
      overflow: hidden;
      border: 1px solid #ececec;
      border-radius: 7px;
    }

    .equipment-table th {
      padding: 8px 10px;
      font-size: 9px;
      font-weight: 700;
      letter-spacing: 0.5px;
      color: #ffffff;
      text-align: left;
      text-transform: uppercase;
      background: #fc2800;
      border: 0;
    }

    .equipment-table td {
      padding: 8px 10px;
      border: 0;
      border-bottom: 1px solid #ececec;
    }

    .equipment-table tr:last-child td {
      border-bottom: 0;
    }

    .equipment-type {
      font-weight: 700;
      color: #030202;
    }

    .quantity-badge {
      display: inline-block;
      min-width: 27px;
      padding: 3px 8px;
      font-size: 10px;
      font-weight: 700;
      color: #ffffff;
      text-align: center;
      background: #030202;
      border-radius: 10px;
    }

    /* =========================================================
       ITEMS TABLE
       ========================================================= */

    .items-table {
      width: 100%;
      margin-top: 5px;
      border-collapse: collapse;
      page-break-inside: auto;
    }

    .items-table thead {
      display: table-header-group;
    }

    .items-table tr {
      page-break-inside: avoid;
      page-break-after: auto;
    }

    .items-table th {
      padding: 9px 10px;
      font-size: 9px;
      font-weight: 700;
      letter-spacing: 0.4px;
      color: #ffffff;
      text-align: left;
      text-transform: uppercase;
      background: #030202;
      border: 1px solid #030202;
    }

    .items-table td {
      padding: 9px 10px;
      vertical-align: middle;
      border: 1px solid #ececec;
    }

    .items-table tbody tr:nth-child(even) {
      background: #fafafa;
    }

    .item-description {
      font-weight: 600;
      color: #030202;
    }

    .item-notes {
      display: block;
      margin-top: 3px;
      font-size: 9px;
      font-weight: 400;
      color: #777777;
    }

    .right {
      text-align: right !important;
    }

    .center {
      text-align: center !important;
    }

    /* =========================================================
       TOTAL
       ========================================================= */

    .totals-wrapper {
      width: 100%;
      margin-top: 13px;
      border-collapse: collapse;
    }

    .totals-wrapper td {
      padding: 0;
      border: 0;
    }

    .total-spacer {
      width: 58%;
    }

    .total-card {
      width: 42%;
      overflow: hidden;
      border-radius: 7px;
      background: #fc2800;
    }

    .total-card-inner {
      padding: 12px 14px;
      color: #ffffff;
    }

    .total-label {
      display: block;
      margin-bottom: 2px;
      font-size: 9px;
      font-weight: 700;
      letter-spacing: 0.7px;
      text-transform: uppercase;
      opacity: 0.85;
    }

    .total-value {
      font-size: 20px;
      font-weight: 800;
    }

    /* =========================================================
       CONDITIONS
       ========================================================= */

    .conditions {
      width: 100%;
      margin-top: 5px;
      border-collapse: separate;
      border-spacing: 0 7px;
    }

    .conditions td {
      padding: 0;
      border: 0;
    }

    .condition {
      padding: 10px 12px;
      border: 1px solid #e6e6e6;
      border-left-width: 5px;
      border-radius: 5px;
      background: #ffffff;
      page-break-inside: avoid;
    }

    .condition-includes {
      border-left-color: #fc2800;
      background: #fff5f2;
    }

    .condition-subject {
      border-left-color: #030202;
      background: #f7f7f7;
    }

    .condition-excludes {
      border-left-color: #fc2800;
      background: #fff1f1;
    }

    .condition-title {
      display: block;
      margin-bottom: 5px;
      font-size: 10px;
      font-weight: 800;
      text-transform: uppercase;
    }

    .condition-includes .condition-title {
      color: #fc2800;
    }

    .condition-subject .condition-title {
      color: #030202;
    }

    .condition-excludes .condition-title {
      color: #fc2800;
    }

    .condition-content {
      white-space: pre-line;
      font-size: 10px;
      line-height: 1.55;
      color: #333333;
    }

    /* =========================================================
       VALIDITY
       ========================================================= */

    .validity {
      margin-top: 17px;
      padding: 9px 12px;
      font-size: 10px;
      color: #333333;
      text-align: center;
      background: #fafafa;
      border: 1px solid #e6e6e6;
      border-radius: 5px;
    }

    .validity strong {
      color: #030202;
    }

    /* =========================================================
       FOOTER
       ========================================================= */

    .footer {
      margin-top: 25px;
      padding-top: 10px;
      font-size: 9px;
      line-height: 1.5;
      color: #777777;
      text-align: center;
      border-top: 1px solid #e6e6e6;
    }
  </style>
</head>

<body>

  <table class="header">
    <tr>
      <td class="brand-area">
        <div class="brand">Grupo Castro Fallas</div>
        <div class="brand-subtitle">Logística Integral</div>
        <div class="brand-line"></div>

        <div class="company-information">
          {{company.website}}<br />
          {{company.email}}<br />
          {{company.phone}}
        </div>
      </td>

      <td class="quote-area">
        <div class="quote-label">Cotización</div>
        <div class="quote-number">{{rate.quoteNumber}}</div>

        <div class="quote-meta">
          <strong>Cliente:</strong> {{rate.clientName}}<br />
          <strong>Fecha:</strong> {{generated.date}}<br />
          <strong>IDTRA:</strong> {{rate.idtraNumber}}
        </div>
      </td>
    </tr>
  </table>

  <div class="header-divider"></div>

  <section class="route-card">
    <div class="route-header">
      <div class="route-label">Ruta de la operación</div>
      <div class="route-value">{{rate.route}}</div>
    </div>

    <table class="route-details">
      <tr>
        <td>
          <span class="data-label">POL · Puerto de origen</span>
          <span class="data-value">{{rate.pol}}</span>
        </td>

        <td>
          <span class="data-label">POE · Puerto de salida</span>
          <span class="data-value">{{rate.poe}}</span>
        </td>

        <td>
          <span class="data-label">POD · Puerto de destino</span>
          <span class="data-value">{{rate.pod}}</span>
        </td>
      </tr>
    </table>
  </section>

  <section class="section">
    <h2 class="section-title">Información de la operación</h2>

    <table class="information-grid">
      <tr>
        <td>
          <span class="data-label">Naviera</span>
          <span class="data-value">{{rate.carrier}}</span>
        </td>


        <td>
          <span class="data-label">Tránsito</span>
          <span class="data-value">{{rate.transitTime}}</span>
        </td>

        <td>
          <span class="data-label">Días libres</span>
          <span class="data-value">{{rate.freeDays}}</span>
        </td>
      </tr>
    </table>
  </section>

  <section class="section">
    <h2 class="section-title">Equipos cotizados</h2>

    <table class="equipment-table">
      <thead>
        <tr>
          <th>Tipo de contenedor</th>
          <th class="center">Cantidad</th>
        </tr>
      </thead>

      <tbody>
        {{#each containers}}
        <tr>
          <td class="equipment-type">{{containerType}}</td>
          <td class="center">
            <span class="quantity-badge">{{quantity}}</span>
          </td>
        </tr>
        {{/each}}
      </tbody>
    </table>
  </section>

  <section class="section">
    <h2 class="section-title">Detalle de la cotización</h2>

    <table class="items-table">
      <thead>
        <tr>
          <th style="width: 46%;">Concepto</th>
          <th class="center" style="width: 12%;">Cantidad</th>
          <th class="right" style="width: 21%;">Precio unitario</th>
          <th class="right" style="width: 21%;">Total</th>
        </tr>
      </thead>

      <tbody>
        {{#each items}}
        <tr>
          <td>
            <span class="item-description">{{description}}</span>
            <span class="item-notes">{{notes}}</span>
          </td>

          <td class="center">{{quantity}}</td>
          <td class="right">{{unitSale}}</td>
          <td class="right"><strong>{{lineTotal}}</strong></td>
        </tr>
        {{/each}}
      </tbody>
    </table>

    <table class="totals-wrapper">
      <tr>
        <td class="total-spacer"></td>
        <td class="total-card">
          <div class="total-card-inner">
            <span class="total-label">Total de la cotización</span>
            <span class="total-value">{{rate.total}}</span>
          </div>
        </td>
      </tr>
    </table>
  </section>

  <section class="section">
    <h2 class="section-title">Condiciones comerciales</h2>

    <table class="conditions">
      <tr>
        <td>
          <div class="condition condition-includes">
            <span class="condition-title">Incluye</span>
            <div class="condition-content">{{rate.includes}}</div>
          </div>
        </td>
      </tr>

      <tr>
        <td>
          <div class="condition condition-subject">
            <span class="condition-title">Sujeto a</span>
            <div class="condition-content">{{rate.subjectTo}}</div>
          </div>
        </td>
      </tr>

      <tr>
        <td>
          <div class="condition condition-excludes">
            <span class="condition-title">No incluye</span>
            <div class="condition-content">{{rate.excludes}}</div>
          </div>
        </td>
      </tr>
    </table>
  </section>

  <div class="validity">
    <strong>Vigencia de la oferta:</strong>
    {{rate.validFrom}} al {{rate.validTo}}
  </div>

  <footer class="footer">
    <strong>Grupo Castro Fallas</strong><br />
    Cotización {{rate.quoteNumber}} ·
    Generada el {{generated.date}} a las {{generated.time}}.<br /><br />

    Válida para carga general, no peligrosa, no sobrepesada, no sobredimensionada.<br />
    *Sujeta a disponibilidad de espacio.<br />
    <strong>APLICA PARA CARGA ABORDO EN LA FECHA DE VALIDEZ DE LA TARIFA</strong><br />
    Tarifa sujeta a costos por inspección.<br />
    No incluye demoras de contenedor, demoras de chasis.<br />
    Seguro de carga (se puede agregar, según instrucciones del cliente).<br />
    Si su carga lleva producto IMO favor indicarlo previamente y compartir fichas MSDS para indicarle si la carga puede ser aceptada y recargo por este ITEM.<br />
    Carga debe de estar correctamente embalada para su transporte internacional (LA CARGA DEBE SER ENTREGADA DEBIDAMENTE EMBALADA Y ROTULADA).<br />
    TT estimado aprox sujeto a conexiones.<br />
    La cancelación de un embarque u operación que tenga BOOKING/CONTENEDOR ASIGNADO será sujeta a una penalización de USD 1.000,00 por Booking.<br />
    Grupo Castro Fallas no asume responsabilidad por carga no asegurada.<br />
    Email: pricing@grupocastrofallas.com / Whatsapp: +506 7078-6941<br />
    <strong>Muchas gracias por permitirnos COTIZARLE</strong>
  </footer>

</body>
</html>
                $html$,
                page_size = 'A4',
                orientation = 'Portrait',
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW()
            WHERE code = 'pricing-fcl-client-quote';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The previous template is intentionally not restored. This migration establishes
        // the approved client-facing Pricing layout as the canonical template.
    }
}
