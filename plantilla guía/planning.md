🟢 Nivel 1: Entidades Maestras (Fáciles)

Son tablas casi idénticas a lo que hemos hecho con los camiones, perfectas para coger ritmo:

    Productos (producto): El catálogo de lo que fabricamos. Necesita su CRUD con nombre, descripcion, peso_unitario y volumen_unitario. Estos dos últimos datos serán vitales más adelante para saber si caben en un camión.

    Conductores (conductor): El personal que conduce. Solo requiere datos básicos como dni, nombre, apellidos y telefono.

🟡 Nivel 2: Entidades con Direcciones (Nivel Medio)

Igual que hicimos con los almacenes, estas entidades necesitan estar conectadas a la tabla direccion:

    Clientes (cliente): A quién le vendemos. Necesitan su nombre, teléfono, email y, por supuesto, seleccionar una dirección del modal que ya programamos.

    Fábricas (fabrica): El origen de todo. Muy parecido al almacén, es el lugar de donde salen los productos y también requiere una direccion_id.

🔴 Nivel 3: El "Core" del Negocio (Avanzado)

Aquí es donde la aplicación dejará de ser un simple "guardador de datos" y empezará a funcionar como un auténtico ERP logístico:

    Pedidos (pedido y pedido_detalle): Un cliente nos compra algo. Tendremos que hacer una pantalla de "Maestro-Detalle" donde el usuario seleccione un cliente, la fecha, y luego vaya añadiendo líneas de productos y cantidades.

    Cargas (carga y carga_pedido): ¡El monstruo final! 🐉 Aquí cruzaremos todo. Tendremos que elegir un camion_id, un conductor_id, un origen (origen_almacen_id) y manejar ese famoso "Arco Exclusivo" para el destino: o va a un almacén (destino_almacen_id), o va a un cliente (destino_cliente_id), pero nunca a ambos. Además, guardaremos los "Snapshots" históricos de los nombres.

    Entregas (entrega): La pantalla final que usarán los conductores para marcar si un pedido ha sido entregado correctamente o si ha habido incidencias, rellenando el campo observaciones y el estado



    añadir borrado logico a productos y conductores