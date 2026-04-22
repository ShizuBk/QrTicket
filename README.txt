# Título del Proyecto
QrTicket

## Descripción
Este proyecto consiste en un sistema para compra de boletos online generando tickets QR.

## Características
-El proyecto consta de una interfaz web que permite el registro y simulación de cobro y una API que recibe los datos de compra y del usuario generando un PDF que se guarda en archivos locales para poder ser descargados por el usuario

#Cambios
-Se crea el módulo de generación de QR y PDF
-Se crea la API para el control de los servicios de generación de tickets
-Se crea la Interfaz web para su uso con la API
-Se modifica el formato de salida del ticket en PDF
-Se agrega entity framework para la gestión de los datos con pgsql
-Se agregó el módulo para la verificación de tickets
-Agregado el módulo de API para backoffice
