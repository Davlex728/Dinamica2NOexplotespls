# Hover-Grúa de Rescate 🚁

Proyecto final para la asignatura de Física 3D centrado en la creación de un vehículo basado íntegramente en simulaciones físicas utilizando el motor Unity 6. El jugador controla un dron tipo grúa que debe navegar por una ciudad nocturna, recoger cargas mediante un sistema de físicas de péndulo y superar obstáculos ambientales.

## 📋 Tabla de Contenidos
- [Objetivo del Proyecto](#objetivo-del-proyecto)
- [Tecnologías y Herramientas](#tecnologías-y-herramientas)
- [Mecánicas e Implementación Física](#mecánicas-e-implementación-física)
- [Decisiones de Diseño y Soluciones Técnicas](#decisiones-de-diseño-y-soluciones-técnicas)
- [Instrucciones de Juego](#instrucciones-de-juego)
- [Equipo de Desarrollo](#equipo-de-desarrollo)

---

## 🎯 Objetivo del Proyecto
Desarrollar un juego completo donde la mecánica central gire en torno a la física, integrando la mayor cantidad de elementos vistos durante el curso (fuerzas, momentos, físicas articulares, fluidos, etc.). El objetivo secundario es mantener la estabilidad de la simulación física evitando bugs o comportamientos erráticos en el Rigidbody.

## 🛠 Tecnologías y Herramientas
* **Motor:** Unity 6 (Renderizado con HDRP para la iluminación nocturna)
* **Lenguaje:** C#
* **Input:** New Input System de Unity
* **Gestión y Control de Versiones: GitHub y GitHub LFS

---

## ⚙️ Mecánicas e Implementación Física

El núcleo del juego recae sobre dos scripts principales que gestionan la interacción física:

### 1. El Dron (`GoofyDroneEventsController.cs`)
El dron es un objeto `Rigidbody` puro sin animaciones predefinidas [code_file:1]. Su movimiento se logra aplicando fuerzas en 4 puntos distintos simulando propulsores:
* **Empuje vectorial:** Uso de `AddForceAtPosition` para empujar cada esquina del dron independientemente, logrando un control dinámico de la inclinación (Pitch y Roll) [code_file:14].
* **Control de Yaw:** Implementación de `AddRelativeTorque` asociado al joystick para permitir al jugador rotar la cámara de forma controlada [code_file:14].
* **Estabilizador giroscópico:** Se utiliza un cálculo de producto cruzado (`Vector3.Cross(transform.up, Vector3.up)`) para generar un torque corrector que evita que el dron vuelque con facilidad [code_file:14].

### 2. La Grúa y la Carga (`CraneHook.cs`)
Para la recolección de objetos se diseñó un sistema de físicas elásticas:
* **Detección Radial:** Un `Physics.OverlapSphere` detecta los objetos con el tag "Load" en un radio definido [code_file:11].
* **Conexión Dinámica (Joints):** Al detectar la carga, el script crea dinámicamente un `SpringJoint` que conecta el `Rigidbody` de la carga al dron [code_file:11].
* **Visualización:** Un componente `LineRenderer` se actualiza en cada frame, trazando una línea entre los puntos de anclaje (Anchor y Connected Anchor) para simular el cable tensor [code_file:11].

### 3. Entorno y Físicas Secundarias
* **Zonas de  viento:** Áreas con `Triggers` que aplican empuje direccional---

## 💡 Decisiones de Diseño y Soluciones Técnicas

Durante el desarrollo nos enfrentamos a varios retos de simulación que resolvimos de la siguiente manera:

1. **Problema del control inestable:** Inicialmente, controlar la inclinación usando 4 propulsores individuales resultaba incontrolable y caótico.
   * *Solución:* Modificamos por código el centro de masas del Rigidbody (`rb.centerOfMass = new Vector3(0, -0.5f, 0);`) para bajar el punto de equilibrio, logrando que el dron se comporte como un péndulo estabilizado, mejorando drásticamente la jugabilidad [code_file:14].
2. **Rotación sobre el eje vertical (Yaw):** Intentar girar el dron aplicando más fuerza a los propulsores diagonales generaba inestabilidad y conflictos con el estabilizador.
   * *Solución:* Añadimos un input exclusivo mediante un eje del joystick acoplado a un `AddRelativeTorque` para aislar el giro de la cámara de la física de vuelo principal [code_file:14].


---

## 🎮 Instrucciones de Juego

**Objetivo:** 
Navega por la ciudad guiándote por los pilares de luz, localiza la zona de rescate y engancha la carga utilizando tu gancho físico para transportarla a la meta.

**Controles (Mando / Gamepad):**
* **Gatillos Izquierdo/Derecho:**Controlan los propulsores delanteros (bajar morro).
* **Bumpers (L1/R1):** Controlan los propulsores traseros (elevar morro).
* **Botón de Empuje Global (South button):** Activa los 4 propulsores al mismo tiempo para elevación vertical rápida.
* **Joystick Izquierdo/Derecho (Eje X):** Gira el morro del dron sobre sí mismo (Yaw) para orientar la cámara.
* **Botón de Acción (East button):** Lanza/Recoge el gancho magnético al acercarte a una carga.

---

*Video del Gameplay: https://youtu.be/2SE0mY-8f4Q*