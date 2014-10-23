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
      // ESTADOS EN LOS QUE SE ENCONTRARÁ LA EJECUCIÓN DEL PROGRAMA:
      public enum ESTADO_MOVIMIENTO {
         QUIET, GO_BACK, GO_FORWARD, ERROR, BEHIND, COMPLETE
      };

      private ESTADO_MOVIMIENTO estado = ESTADO_MOVIMIENTO.QUIET;
      private float cadera_ini_X, cadera_ini_Z;
      private double distancia = 10;           // Distancia en centímetros para completar el movimiento
      private double error = 0.05;             // Porcentaje de error para los cambios de estado (5%)
      private double dist_Z;                   // Coordenada Z del punto a la distancia que hay que alcanzar

      // COLORES DEL ESQUELETO QUE SE VAN A EMPLEAR
      private readonly Brush hueso_movCorrecto = Brushes.Green;
      private readonly Pen articulacion_movCorrecto = new Pen(Brushes.Green, 6);
      private readonly Brush hueso_distAlcanzada = Brushes.Yellow;
      private readonly Pen articulacion_distAlcanzada = new Pen(Brushes.Yellow, 6);
      private readonly Brush hueso_error = Brushes.Red;
      private readonly Pen articulacion_error = new Pen(Brushes.Red, 6);
      private readonly Brush hueso_completado = Brushes.Blue;
      private readonly Pen articulacion_completado = new Pen(Brushes.Blue, 6);

      
      public void prueba_coordenadas(Skeleton skel)
      {
         Joint cad_cen = skel.Joints[JointType.HipCenter];

         float cadera_X = cad_cen.Position.X * 100;
         float cadera_Y = cad_cen.Position.Y * 100;
         float cadera_Z = cad_cen.Position.Z * 100;

         textBox1.Clear();
         textBox1.AppendText("\nCadera\n");
         textBox1.AppendText("X: " + cadera_X.ToString() + "\n");
         textBox1.AppendText("Y: " + cadera_Y.ToString() + "\n");
         textBox1.AppendText("Z: " + cadera_Z.ToString() + "\n");

         textBox1.AppendText("\nESTADO: " + estado + "\n");
      }

      // Devuelve true si se ha completado el movimiento correctamente
      // y false en caso contrario
      public bool movimiento_39(Skeleton skel, double distancia)
      {
         Joint cad_cen = skel.Joints[JointType.HipCenter];

         // TRABAJAMOS EN CENTÍMETROS
         float cadera_X = cad_cen.Position.X * 100;
         float cadera_Z = cad_cen.Position.Z * 100;

         if (estado == ESTADO_MOVIMIENTO.QUIET)
         {
            // Almacenamos las coordenadas iniciales que detecta Kinect
            this.cadera_ini_X = cadera_X;
            this.cadera_ini_Z = cadera_Z;

            // Calculamos la coordenada Z del punto que está a la distancia deseada
            this.dist_Z = cadera_ini_Z + this.distancia;
            
            estado = ESTADO_MOVIMIENTO.GO_BACK;
         }
         else if (estado == ESTADO_MOVIMIENTO.GO_BACK)
         {
            if ((cadera_Z <= cadera_ini_Z - (cadera_ini_Z * this.error)) ||
               (cadera_X > cadera_ini_X + (cadera_ini_X * this.error + 2)) || (cadera_X < cadera_ini_X - (cadera_ini_X * this.error + 2)))
            {
               estado = ESTADO_MOVIMIENTO.ERROR;
               textBox3.Clear();
               textBox3.AppendText("Demasiado adelante, derecha o izquierda");
            }
            else if ( (cadera_Z >= this.dist_Z) && cadera_Z < this.dist_Z+(this.dist_Z*this.error) )
               estado = ESTADO_MOVIMIENTO.BEHIND;
         }
         else if (estado == ESTADO_MOVIMIENTO.BEHIND)
         {
            if (cadera_Z > this.dist_Z + (this.dist_Z * this.error))
            {
               estado = ESTADO_MOVIMIENTO.ERROR;
               textBox3.Clear();
               textBox3.AppendText("Demasiado atrás");
            }
            else if (cadera_Z < this.dist_Z-(this.dist_Z*this.error))
               estado = ESTADO_MOVIMIENTO.GO_FORWARD;
         }
         else if (estado == ESTADO_MOVIMIENTO.GO_FORWARD)
         {
            if ((cadera_X > cadera_ini_X + (cadera_ini_X * this.error + 2)) || (cadera_X < cadera_ini_X - (cadera_ini_X * this.error + 2)))
            {
               estado = ESTADO_MOVIMIENTO.ERROR;
               textBox3.Clear();
               textBox3.AppendText("Derecha o Izquierda");
            }
            else if (cadera_Z < cadera_ini_Z && cadera_Z > cadera_ini_Z - (cadera_ini_Z * this.error))
            {
               estado = ESTADO_MOVIMIENTO.COMPLETE;
               return true;
            }
         }
         else if (estado == ESTADO_MOVIMIENTO.ERROR)
         {
            if ( cadera_Z > cadera_ini_Z-(cadera_ini_Z*this.error) && cadera_Z < cadera_ini_Z+(cadera_ini_Z*this.error) &&
               cadera_X > cadera_ini_X-(cadera_ini_X*this.error+2) && cadera_X < cadera_ini_X+(cadera_ini_X*this.error+2) )
               estado = ESTADO_MOVIMIENTO.GO_BACK;
         }
         
         textBox2.Clear();
         textBox2.AppendText((this.dist_Z-cadera_Z).ToString());
         return false;
      }
   }
}
