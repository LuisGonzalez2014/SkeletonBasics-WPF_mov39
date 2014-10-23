//------------------------------------------------------------------------------
// <copyright file="movimiento_39.cs" autor="Luis Alejandro González Borrás">
//     Copyright Luis Alejandro González Borrás.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
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
      /// Coordenadas X y Z de la posición inicial de la cadera (hipcenter)
      /// </summary>
      private float cadera_ini_X, cadera_ini_Z;

      /// <summary>
      /// Distancia en centímetros para completar el movimiento
      /// </summary>
      private double distancia = 10;

      /// <summary>
      /// Porcentaje de error admitido para la precisión de la detección (5%)
      /// </summary>
      private double error = 0.05;

      /// <summary>
      /// Coordenada Z del punto hasta la distancia que hay que alcanzar
      /// </summary>
      private double dist_Z;

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
      /// Imprime en el cuadro de texto 1 las coordenadas de la cadera en tiempo real
      /// así como el estado de ejecución actual.
      /// </summary>
      /// <param name="skel">esqueleto detectado</param>
      public void prueba_coordenadas(Skeleton skel)
      {
         // Almacenamos el punto central de la cadera detectado
         Joint cad_cen = skel.Joints[JointType.HipCenter];

         // Obtenemos las coordenadas del punto anterior en centímetros
         float cadera_X = cad_cen.Position.X * 100;
         float cadera_Y = cad_cen.Position.Y * 100;
         float cadera_Z = cad_cen.Position.Z * 100;

         // Imprimimos las coordenadas en el cuadro de texto 1
         textBox1.Clear();
         textBox1.AppendText("\nCadera\n");
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
      /// <returns>confirmación o negación del movimiento</returns>
      public bool movimiento_39(Skeleton skel, double distancia)
      {
         // Almacenamos el punto central de la cadera detectado
         Joint cad_cen = skel.Joints[JointType.HipCenter];

         // Obtenemos las coordenadas X y Z del punto anterior en centímetros
         float cadera_X = cad_cen.Position.X * 100;
         float cadera_Z = cad_cen.Position.Z * 100;

         // Comprobación del estado actual de ejecución
         if ( estado == ESTADO_MOVIMIENTO.QUIET )
         {
            // Obtenemos las coordenadas iniciales X y Z de la cadera en centímetros
            this.cadera_ini_X = cadera_X;
            this.cadera_ini_Z = cadera_Z;

            // Calculamos la coordenada Z del punto que está a la distancia deseada
            this.dist_Z = cadera_ini_Z + this.distancia;
            
            estado = ESTADO_MOVIMIENTO.GO_BACK;
         }
         else if ( estado == ESTADO_MOVIMIENTO.GO_BACK )
         {
            // Si estando en la posición inicial, movemos la cadera hacia adelante o hacia los lados, el movimiento es erroneo
            if ( (cadera_Z <= cadera_ini_Z - (cadera_ini_Z * this.error)) ||
               (cadera_X > cadera_ini_X+(cadera_ini_X*this.error+2)) || (cadera_X < cadera_ini_X-(cadera_ini_X*this.error+2)) )
            {
               estado = ESTADO_MOVIMIENTO.ERROR;
               // Imprimimos el error cometido en el cuadro de texto 3
               textBox3.Clear();
               textBox3.AppendText("Demasiado adelante, derecha o izquierda");
            }
            // Si estando en la posición inicial, movemos la cadera hacia atrás el movimiento está siendo correcto
            else if ( (cadera_Z >= this.dist_Z) && cadera_Z < this.dist_Z+(this.dist_Z*this.error) )
               estado = ESTADO_MOVIMIENTO.BEHIND;
         }
         else if ( estado == ESTADO_MOVIMIENTO.BEHIND )
         {
            // Si estando en la distancia requerida, seguimos retrasando la cadera, el movimiento es erroneo
            if ( cadera_Z > this.dist_Z + (this.dist_Z * this.error) )
            {
               estado = ESTADO_MOVIMIENTO.ERROR;
               // Imprimimos el error cometido en el cuadro de texto 3
               textBox3.Clear();
               textBox3.AppendText("Demasiado atrás");
            }
            // Si estando en la distancia requerida, avanzamos la cadera hacia adelante, el movimiento está siendo correcto
            else if (cadera_Z < this.dist_Z-(this.dist_Z*this.error))
               estado = ESTADO_MOVIMIENTO.GO_FORWARD;
         }
         else if ( estado == ESTADO_MOVIMIENTO.GO_FORWARD )
         {
            // Si volviendo a la posición inicial, desplazamos la cadera hacia los lados, el movimiento es erroneo
            if ( (cadera_X > cadera_ini_X + (cadera_ini_X * this.error + 2)) ||
               (cadera_X < cadera_ini_X - (cadera_ini_X * this.error + 2)) )
            {
               estado = ESTADO_MOVIMIENTO.ERROR;
               // Imprimimos el error cometido en el cuadro de texto 3
               textBox3.Clear();
               textBox3.AppendText("Derecha o Izquierda");
            }
            // Si volviendo a la posición inicial, avanzamos hacia la posición de partida, el movimiento ha sido correcto
            else if ( cadera_Z < cadera_ini_Z && cadera_Z > cadera_ini_Z - (cadera_ini_Z * this.error) )
            {
               estado = ESTADO_MOVIMIENTO.COMPLETE;
               return true;
            }
         }
         else if ( estado == ESTADO_MOVIMIENTO.ERROR )
         {
            // Si hemos realizado un movimiento erroneo y volvemos a la posición inicial, se reinicia el proceso
            if ( cadera_Z > cadera_ini_Z-(cadera_ini_Z*this.error) && cadera_Z < cadera_ini_Z+(cadera_ini_Z*this.error) &&
               cadera_X > cadera_ini_X-(cadera_ini_X*this.error+2) && cadera_X < cadera_ini_X+(cadera_ini_X*this.error+2) )
               estado = ESTADO_MOVIMIENTO.GO_BACK;
         }

         // Imprimimos la distancia restante hacia atrás en el cuadro de texto 2
         textBox2.Clear();
         textBox2.AppendText((this.dist_Z-cadera_Z).ToString());
         return false;
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
