# CASOR — El modelo de datos explicado en simple

*Documento para presentar el diagrama (MER) a dueños, químicos farmacéuticos y personas no técnicas. Sin jerga: solo lo que el sistema recuerda y por qué.*

---

## ¿Qué estamos mirando?

El diagrama es **el plano de la memoria del sistema**. Cada caja es como un archivador donde CASOR guarda un tipo de información (productos, ventas, recetas...), y cada línea indica que dos archivadores están conectados. Cuando una línea termina en una "pata de gallo" (⋔) significa "uno tiene muchos": una venta tiene muchos productos, un producto tiene muchos lotes.

Si el plano está bien hecho, el sistema puede responder cualquier pregunta del negocio en segundos. Si está mal hecho, hay preguntas que nunca podrá responder. Por eso lo diseñamos antes de programar.

---

## La historia que cuenta el diagrama, en 8 capítulos

### 1. El catálogo: qué vendemos

**PRODUCTO** es la ficha de cada artículo: su código de barras (lo que lee el escáner), nombre, precio de venta al público, y si es un medicamento, su compuesto (**PRINCIPIO ACTIVO**) y si requiere receta. Cada vez que cambia un precio, el precio viejo no se borra: queda en **PRECIO HISTÓRICO**, con fecha y quién lo cambió. Así siempre se puede responder "¿a cuánto vendíamos esto en marzo?".

**El compuesto es una pieza clave:** si un cliente pide un remedio de marca que está caro o agotado, el sistema le muestra al cajero al instante las alternativas con el mismo compuesto y dosis. Eso hoy depende de la memoria del vendedor; con CASOR es automático. Venta que hoy se pierde, mañana se recupera.

### 2. El inventario: no contamos remedios, contamos LOTES

Aquí está la diferencia más importante con un sistema genérico. CASOR no guarda "hay 89 Paracetamol": guarda **"hay 47 del lote F2231 que vence en marzo de 2027, y 42 del lote G1105 que vence en octubre"** (**LOTE** y **STOCK LOTE**).

¿Por qué importa? Tres razones que son plata:

- **Se vende primero lo que vence primero.** El sistema le dice al cajero de qué caja sacar. Los remedios vencidos en el estante — pérdida pura — se reducen drásticamente.
- **Si la autoridad (ISP) retira un lote defectuoso del mercado**, hoy hay que revisar estantes a mano. Con CASOR la respuesta toma un minuto: cuántos tenemos, dónde están y a quién le vendimos de ese lote.
- **Cada lote recuerda lo que costó** al comprarlo al proveedor. Por eso el sistema puede calcular la ganancia real de cada venta, no solo cuánto entró.

### 3. La venta: la boleta y todo lo que hay detrás

Cuando el cajero cobra, se crea una **VENTA** con sus líneas (**DETALLE DE VENTA**: qué producto, cuántos, a qué precio, *de qué lote salió*) y sus **PAGOS**. Ojo con el plural: una venta puede pagarse mitad efectivo, mitad tarjeta — el sistema lo registra separado, porque al cerrar la caja hay que cuadrar cada medio de pago por su lado.

El precio queda **congelado** en la boleta: si mañana sube el precio de lista, las ventas de ayer no cambian. Y si un cliente devuelve algo, la **nota de crédito** queda amarrada a la venta original — nada de devoluciones fantasma.

La boleta puede ser anónima (lo normal) o asociarse a un **CLIENTE** con nombre, para convenios, cotizaciones y facturas.

### 4. La caja: quién es responsable de la plata

Cada turno de cajero es una **SESIÓN DE CAJA**: se abre con un fondo, se registran todas las ventas del turno, y al cerrar el sistema compara **lo que el cajero contó** contra **lo que debería haber** según las ventas. La diferencia — el descuadre — queda registrada con nombre y apellido. El cierre Z de la noche congela el día completo: después de eso, nadie puede tocar las ventas de esa jornada.

### 5. Las recetas: el libro legal se llena solo

Cuando se vende un medicamento controlado, el sistema exige registrar al médico y al paciente (**RECETA RETENIDA**) y le asigna automáticamente el número correlativo del libro que exige la ley. Lo que hoy es un cuaderno manual que quita tiempo al químico farmacéutico, pasa a ser un clic — y el libro sale impreso o exportado cuando el ISP lo pida. Los datos del paciente van cifrados: aunque alguien robara el disco del servidor, no puede leerlos.

### 6. Los folios: las boletas son legales

Cada boleta electrónica necesita un número autorizado por el SII (folio). El sistema administra esos números (**FOLIO CAF**): los reparte entre las cajas, avisa cuando quedan pocos, y garantiza que jamás se repita uno — porque un folio duplicado es un problema legal, no informático.

### 7. Las personas y los permisos

**USUARIO** guarda a cada persona con su rol: el **cajero** solo puede vender y cerrar su caja; el **administrador** puede todo (cambiar precios, crear productos, ver reportes). Cada acción sensible — anular una venta, cambiar un precio, ajustar inventario — queda escrita en la **AUDITORÍA** con quién, cuándo y qué. Esa bitácora no se puede borrar ni editar: es la memoria legal del negocio.

### 8. Preparado para crecer

**EMPRESA** y **SUCURSAL** existen desde el día uno: hoy es una farmacia con un local, pero abrir un segundo local (o vender el sistema a otra farmacia) no requiere rediseñar nada — el inventario, las cajas y los reportes ya saben trabajar por sucursal.

---

## Las preguntas que este diseño responde en segundos

- ¿Cuánto vendimos hoy, y cuánto **ganamos** (no solo cuánto entró)?
- ¿Qué remedios vencen en los próximos 90 días y cuánta plata hay parada en ellos?
- ¿Cuánto vendió cada vendedor este mes?
- ¿Cuadró la caja de anoche? ¿Y la del martes pasado?
- El ISP retiró el lote X del mercado: ¿tenemos? ¿vendimos? ¿a quién?
- ¿Cuáles son los 20 productos que dejan más margen?
- ¿Quién anuló esa venta del viernes y por qué?
- ¿Cuántas boletas nos quedan autorizadas por el SII?

Si alguna pregunta del negocio no está en esta lista, este es el momento de decirla — agregar memoria al sistema ahora es dibujar una caja más en el plano; agregarla en un año es cirugía.

---

## Sobre el archivo técnico (casor_schema.sql)

Ese archivo es este mismo plano escrito en el idioma que entiende la base de datos (PostgreSQL). Contiene las 19 "cajas" del diagrama convertidas en tablas, más las reglas de protección que el motor hace cumplir solo: el stock nunca puede quedar negativo, un folio nunca puede repetirse, una venta no puede existir sin sus líneas. Es decir: las promesas del diagrama, con candado.

*El detalle técnico completo de cada campo está en `casor_diccionario_datos.md`.*
