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
         EN_INICIAL, A_OBJETIVO, A_INICIAL, ERROR, EN_OBJETIVO, COMPLETADO
      };

      private ESTADO_MOVIMIENTO estado = ESTADO_MOVIMIENTO.EN_INICIAL;
      private float cad_der_X_ini, cad_der_Y_ini, cad_der_Z_ini;
      private float cad_cen_X_ini, cad_cen_Y_ini, cad_cen_Z_ini;
      private float cad_izq_X_ini, cad_izq_Y_ini, cad_izq_Z_ini;
      private double distancia = 15;     // Distancia en centímetros para realizar el movimiento
      private double error = 0.05;       // Porcentaje de error para los cambios de estado (5%).
      private double dist_X, dist_Y, dist_Z;   // Coordenadas del punta hasta la distancia que hay que alcanzar

      /*
      public void prueba_coordenadas(Skeleton skel)
      {
         Joint cad_der = skel.Joints[JointType.HipRight];
         Joint cad_izq = skel.Joints[JointType.HipLeft];
         Joint cad_cen = skel.Joints[JointType.HipCenter];

         float cad_der_X = cad_der.Position.X * 100;
         float cad_der_Y = cad_der.Position.Y * 100;
         float cad_der_Z = cad_der.Position.Z * 100;

         float cad_cen_X = cad_der.Position.X * 100;
         float cad_cen_Y = cad_der.Position.Y * 100;
         float cad_cen_Z = cad_der.Position.Z * 100;

         float cad_izq_X = cad_der.Position.X * 100;
         float cad_izq_Y = cad_der.Position.Y * 100;
         float cad_izq_Z = cad_der.Position.Z * 100;

         textBox1.Clear();
         textBox1.AppendText("\nCadera derecha\n");
         textBox1.AppendText("X: " + cad_der_X.ToString() + "\n");
         textBox1.AppendText("Y: " + cad_der_Y.ToString() + "\n");
         textBox1.AppendText("Z: " + cad_der_Z.ToString() + "\n");

         textBox1.AppendText("\nCadera centro\n");
         textBox1.AppendText("X: " + cad_cen_X.ToString() + "\n");
         textBox1.AppendText("Y: " + cad_cen_Y.ToString() + "\n");
         textBox1.AppendText("Z: " + cad_cen_Z.ToString() + "\n");

         textBox1.AppendText("\nCadera izquierda\n");
         textBox1.AppendText("X: " + cad_izq_X.ToString() + "\n");
         textBox1.AppendText("Y: " + cad_izq_Y.ToString() + "\n");
         textBox1.AppendText("Z: " + cad_izq_Z.ToString() + "\n");
      }
      */

      public void movimiento(Skeleton skel)
      {
         Joint cad_der = skel.Joints[JointType.HipRight];
         Joint cad_izq = skel.Joints[JointType.HipLeft];
         Joint cad_cen = skel.Joints[JointType.HipCenter];

         float cad_der_X = cad_der.Position.X * 100;
         float cad_der_Y = cad_der.Position.Y * 100;
         float cad_der_Z = cad_der.Position.Z * 100;
         float cad_cen_X = cad_der.Position.X * 100;
         float cad_cen_Y = cad_der.Position.Y * 100;
         float cad_cen_Z = cad_der.Position.Z * 100;
         float cad_izq_X = cad_der.Position.X * 100;
         float cad_izq_Y = cad_der.Position.Y * 100;
         float cad_izq_Z = cad_der.Position.Z * 100;

         if (estado == ESTADO_MOVIMIENTO.EN_INICIAL)
         {
            this.cad_der_X_ini = cad_der_X;
            this.cad_der_Y_ini = cad_der_Y;
            this.cad_der_Z_ini = cad_der_Z;
            this.cad_cen_X_ini = cad_cen_X;
            this.cad_cen_Y_ini = cad_cen_Y;
            this.cad_cen_Z_ini = cad_cen_Z;
            this.cad_izq_X_ini = cad_izq_X;
            this.cad_izq_Y_ini = cad_izq_Y;
            this.cad_izq_Z_ini = cad_izq_Z;

            // Calculamos las coordenadas del punto hasta la distancia objetivo con el punto central de la cadera
            this.dist_X = cad_cen_X_ini + this.distancia;
            this.dist_Y = cad_cen_Y_ini + this.distancia;
            this.dist_Z = cad_cen_Z_ini + this.distancia;

            if (cad_cen_Z >= cad_cen_Z_ini)
               estado = ESTADO_MOVIMIENTO.A_OBJETIVO;
            else
               estado = ESTADO_MOVIMIENTO.ERROR;
         }
         else if (estado == ESTADO_MOVIMIENTO.A_OBJETIVO)
         {
            if (cad_cen_Z >= this.dist_Z && (cad_cen_Z <= this.dist_Z+(this.dist_Z*this.error)))
            {
               estado = ESTADO_MOVIMIENTO.EN_OBJETIVO;
            }
            else if (cad_cen_Z > this.dist_Z+(this.dist_Z * this.error))
            {
               estado = ESTADO_MOVIMIENTO.ERROR;
            }
         }
         else if (estado == ESTADO_MOVIMIENTO.EN_OBJETIVO)
         {
            if ( (cad_cen_Z >= cad_cen_Z_ini) && (cad_cen_Z <= this.dist_Z-(this.dist_Z*this.error)) )
            {
               estado = ESTADO_MOVIMIENTO.A_INICIAL;
            }
            else if (cad_cen_Z > this.dist_Z+(this.dist_Z*this.error))
            {
               estado = ESTADO_MOVIMIENTO.ERROR;
            }
         }
         else if (estado == ESTADO_MOVIMIENTO.A_INICIAL)
         {
            if ((cad_cen_Z >= cad_cen_Z_ini) && (cad_cen_Z <= this.dist_Z-(this.dist_Z*this.error)))
            {
               estado = ESTADO_MOVIMIENTO.COMPLETADO;
            }
            else if (cad_cen_Z < cad_cen_Z_ini-(cad_cen_Z_ini*this.error))
            {
               estado = ESTADO_MOVIMIENTO.ERROR;
            }
         }
         else if (estado == ESTADO_MOVIMIENTO.COMPLETADO)
         {

         }
         else if (estado == ESTADO_MOVIMIENTO.ERROR)
         {

         }
      }
   }
}
