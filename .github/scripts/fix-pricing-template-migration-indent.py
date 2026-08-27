from pathlib import Path

path = Path('src/Dhole.Reports.Persistence/Migrations/20260827234500_UpdatePricingFclClientQuoteTemplateLayout.cs')
text = path.read_text(encoding='utf-8')
start_marker = 'html_content = $html$\n'
end_marker = '\n                $html$,'

if start_marker not in text or end_marker not in text:
    raise SystemExit('No se encontró el bloque HTML de la migración.')

prefix, remainder = text.split(start_marker, 1)
html, suffix = remainder.split(end_marker, 1)

# El delimitador raw string de C# está indentado 12 espacios. Todo el contenido
# debe tener al menos esa indentación; el compilador la elimina del valor final,
# por lo que el HTML almacenado queda exactamente como fue entregado.
html = '\n'.join(('            ' + line) if line else '            ' for line in html.split('\n'))
path.write_text(prefix + start_marker + html + end_marker + suffix, encoding='utf-8')
print('Indentación de raw string corregida.')
