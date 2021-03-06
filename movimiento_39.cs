﻿//------------------------------------------------------------------------------
// <copyright file="movimiento_39.cs" autor="Luis Alejandro González Borrás">
//     Copyright Luis Alejandro González Borrás.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
   using System;
   using System.IO;
   using System.Windows;
   using System.Windows.Media;
   using Microsoft.Kinect;

   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      /// <summary>
      /// Estados de ejecución del programa
      /// </summary>
      public enum ESTADO_MOVIMIENTO {
         QUIET, GO_BACK, GO_FORWARD, ERROR, BEHIND, COMPLETE
      };

      /// <summary>
      /// Estado inicial de ejecución del programa (desde la posición en reposo)
      /// </summary>
      private ESTADO_MOVIMIENTO estado = ESTADO_MOVIMIENTO.QUIET;

      /// <summary>
      /// Posición inicial de la cadera (hipcenter)
      /// </summary>
      SkeletonPoint cadera_inicial;

      /// <summary>
      /// Porcentaje de error admitido para la precisión de la detección (5%)
      /// </summary>
      private double error = 0.05;

      /// <summary>
      /// Pincel para dibujar las articulaciones cuando el movimiento es correcto
      /// </summary>
      private readonly Brush articulacion_movCorrecto = Brushes.Green;

      /// <summary>
      /// Pen para dibujar los huesos cuando el movimiento es correcto
      /// </summary>
      private readonly Pen hueso_movCorrecto = new Pen(Brushes.Green, 6);

      /// <summary>
      /// Pincel para dibujar las articulaciones cuando se alcanza la distancia
      /// </summary>
      private readonly Brush articulacion_distAlcanzada = Brushes.Yellow;

      /// <summary>
      /// Pen para dibujar los huesos cuando se alcanza la distancia
      /// </summary>
      private readonly Pen hueso_distAlcanzada = new Pen(Brushes.Yellow, 6);

      /// <summary>
      /// Pincel para dibujar las articulaciones cuando el movimiento es incorrecto
      /// </summary>
      private readonly Brush articulacion_error = Brushes.Red;

      /// <summary>
      /// Pen para dibujar los huesos cuando el movimiento es incorrecto
      /// </summary>
      private readonly Pen hueso_error = new Pen(Brushes.Red, 6);

      /// <summary>
      /// Pincel para dibujar las articulaciones cuando se ha completado el movimiento
      /// </summary>
      private readonly Brush articulacion_completado = Brushes.Blue;

      /// <summary>
      /// Pen para dibujar los huesos cuando se ha completado el movimiento
      /// </summary>
      private readonly Pen hueso_completado = new Pen(Brushes.Blue, 6);

      /// <summary>
      /// Errores permitidos en el desplazamiento lateral de la cadera
      /// <summary>
      double x_error_minimo = 0, x_error_maximo = 0;

      /// <summary>
      /// Imprime en el cuadro de texto 1 las coordenadas de la cadera en tiempo real
      /// así como el estado de ejecución actual.
      /// </summary>
      /// <param name="skel">esqueleto detectado</param>
      public void prueba_coordenadas(Skeleton skel)
      {
         // Almacenamos el punto central de la cadera detectado
         Joint cadera = skel.Joints[JointType.HipCenter];

         // Obtenemos las coordenadas del punto anterior en centímetros
         float cadera_X = cadera.Position.X;
         float cadera_Y = cadera.Position.Y;
         float cadera_Z = cadera.Position.Z;

         // Imprimimos las coordenadas en el cuadro de texto 1
         textBox1.Clear();
         textBox1.AppendText("\nCadera Central\n");
         textBox1.AppendText("X: " + cadera_X.ToString() + "\n");
         textBox1.AppendText("Y: " + cadera_Y.ToString() + "\n");
         textBox1.AppendText("Z: " + cadera_Z.ToString() + "\n");

         // Imprimimos el estado de ejecución actual en el cuadro de texto 1
         textBox1.AppendText("\nESTADO: " + estado + "\n");
      }

      /// <summary>
      /// Indica si se ha realizado correctamente el movimiento hacia atrás de la cadera
      /// </summary>
      /// <param name="skel">esqueleto detectado</param>
      /// <param name="distancia">distancia requerida para realizar el movimiento</param>
      /// <returns>estado del movimiento</returns>
      public int movimiento_39(Skeleton skel, double dist)
      {
         // Variable que indica el estado del movimiento:
         //    0: NO completado
         //    1: error
         //    2: completado
         int salida = 0;

         // Obtenemos la posición actual de la cadera
         SkeletonPoint cadera_actual = skel.Joints[JointType.HipCenter].Position;

         // Distancia desde la posición inicial de la cadera hasta la posición actual
         double dist_actual = 0;

         // Errores permitidos en la distancia actual donde deberá oscilar la distancia calculada
         double dist_error_minimo = dist - (dist * this.error + 0.02);
         double dist_error_maximo = dist + (dist * this.error + 0.02);

         // Comprobación del estado actual de ejecución
         if ( estado == ESTADO_MOVIMIENTO.QUIET )
         {
            // Obtenemos la posición inicial de la cadera
            cadera_inicial = skel.Joints[JointType.HipCenter].Position;
            
            // Calculamos los errores permitidos para el desplazamiento lateral
            x_error_minimo = cadera_inicial.X - (cadera_inicial.X * this.error + 0.02);
            x_error_maximo = cadera_inicial.X + (cadera_inicial.X * this.error + 0.02);

            estado = ESTADO_MOVIMIENTO.GO_BACK;
         }
         else if ( estado == ESTADO_MOVIMIENTO.GO_BACK )
         {
            // Calculamos la distancia de la cadera entre la posición de partida y la actual
            dist_actual = this.distance(cadera_inicial, cadera_actual);

            // Si estando en la posición inicial, movemos la cadera hacia adelante o hacia los lados, el movimiento es erroneo
            // (para el movimiento lateral, en el eje X, se aumenta el error permitido en 2 cm)
            if ( (cadera_actual.Z <= cadera_inicial.Z - (cadera_inicial.Z * this.error)) || (cadera_actual.X >= x_error_maximo)
               || (cadera_actual.X <= x_error_minimo) )
            {
               estado = ESTADO_MOVIMIENTO.ERROR;
               // Imprimimos el error cometido en el cuadro de texto 3
               textBox3.Clear();
               textBox3.AppendText("Demasiado delante, derecha o izquierda");
               salida = 1;
            }
            // Si estando en la posición inicial, movemos la cadera hacia atrás el movimiento está siendo correcto
            else if ( (dist_actual > dist_error_minimo) && (dist_actual < dist_error_maximo) )
               estado = ESTADO_MOVIMIENTO.BEHIND;
         }
         else if ( estado == ESTADO_MOVIMIENTO.BEHIND )
         {
            // Calculamos la distancia de la cadera entre la posición de partida y la actual
            dist_actual = this.distance(cadera_inicial, cadera_actual);

            // Si estando en la distancia requerida, seguimos retrasando la cadera, el movimiento es erroneo
            if (dist_actual > dist_error_maximo)
            {
               estado = ESTADO_MOVIMIENTO.ERROR;
               // Imprimimos el error cometido en el cuadro de texto 3
               textBox3.Clear();
               textBox3.AppendText("Demasiado atrás");
               salida = 1;
            }
            // Si estando en la distancia requerida, avanzamos la cadera hacia adelante, el movimiento está siendo correcto
            else if ( dist_actual < dist_error_minimo )
               estado = ESTADO_MOVIMIENTO.GO_FORWARD;
         }
         else if ( estado == ESTADO_MOVIMIENTO.GO_FORWARD )
         {
            // Calculamos la distancia de la cadera entre la posición de partida y la actual
            dist_actual = this.distance(cadera_inicial, cadera_actual);

            // Si volviendo a la posición inicial, desplazamos la cadera hacia los lados, el movimiento es erroneo
            // (para el movimiento lateral, en el eje X, se aumenta el error permitido en 2 cm)
            if ( (cadera_actual.X >= x_error_maximo) || (cadera_actual.X <= x_error_minimo) )
            {
               estado = ESTADO_MOVIMIENTO.ERROR;
               // Imprimimos el error cometido en el cuadro de texto 3
               textBox3.Clear();
               textBox3.AppendText("Derecha o Izquierda");
               salida = 1;
            }
            // Si volviendo a la posición inicial, avanzamos hacia la posición de partida, el movimiento ha sido correcto
            else if ( dist_actual >= -0.03 && dist_actual <= 0.03 )
            {
               estado = ESTADO_MOVIMIENTO.COMPLETE;
               salida = 2;
            }
         }
         else if ( estado == ESTADO_MOVIMIENTO.ERROR )
         {
            // Calculamos la distancia de la cadera entre la posición de partida y la actual
            dist_actual = this.distance(cadera_inicial, cadera_actual);

            // Si hemos realizado un movimiento erroneo y volvemos a la posición inicial, se reinicia el proceso
            if (dist_actual >= -0.03 && dist_actual <= 0.03 && (cadera_actual.X < x_error_maximo)
               && (cadera_actual.X > x_error_minimo) )
               estado = ESTADO_MOVIMIENTO.GO_BACK;
         }

         // Imprimimos la distancia restante hacia atrás en el cuadro de texto 2
         textBox2.Clear();
         //textBox2.AppendText(((dist-dist_actual)*100).ToString());
         textBox2.AppendText((dist_actual*100).ToString());
         return salida;
      }

      /// <summary>
      /// Devuelve la distancia entre las posiciones de un punto del esqueleto desde un instante inicial al instante final
      /// </summary>
      /// <param name="inicial">posición en el instante inicial</param>
      /// <param name="final">posición en el instante final</param>
      /// <returns>distancia entre ambas posiciones</returns>
      public double distance(SkeletonPoint inicial, SkeletonPoint final)
      {
         double resultado = Math.Sqrt(Math.Abs(Math.Pow((final.X - inicial.X), 2) + Math.Pow((final.Y - inicial.Y), 2) + Math.Pow((final.Z - inicial.Z), 2)));
         
         // Si la posición de la cadera está más adelantada que la posición inicial, la distancia será negativa.
         if ( final.Z < inicial.Z )
            resultado = -resultado;
         return resultado;
      }

      /// <summary>
      /// Maneja la acción al pulsar sobre el botón de reinicialización
      /// </summary>
      /// <param name="sender">object sending the event</param>
      /// <param name="e">event arguments</param>
      private void button1_Click(object sender, RoutedEventArgs e)
      {
         estado = ESTADO_MOVIMIENTO.QUIET;
      }
   }
}
