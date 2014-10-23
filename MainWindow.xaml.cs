﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
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

       bool correct = false;
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        //private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush trackedJointBrush = Brushes.Green;

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        //private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Brush inferredJointBrush = Brushes.Red;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        //private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);
        private readonly Pen inferredBonePen = new Pen(Brushes.Red, 6);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;     /*LO VAMOS A UTILIZAR NOSOTROS TAMBIEN (en lugar de crear otra instancia)*/

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /* INDICA QUÉ BORDES ESTAN CORTANDO ZONAS DEL ESQUELETO */
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        /* EJECUTA LAS TAREAS INICIALES */
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)   // SI HAY AL MENOS UN KINECT CONECTADO COMIENZA...
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)   // SI NO HAY UN KINECT CONECTADO TERMINA...
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        /* EJECUTA LAS TAREAS FINALES */
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            // COMENZAMOS A RECIBIR FRAMES DEL ESQUELETO Y A ALMACENARLOS EN skeletonFrame
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    // PARA CADA ESQUELETO DETECTADO SE HACE LO SIGUIENTE...
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);
                        
                        // SI EL ESQUELETO SE DETECTA DENTRO DEL RANGO QUE TIENE KINECT...
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
<<<<<<< HEAD
                           this.DrawBonesAndJoints(skel, dc);
=======
                            bool correct = this.movimiento_39(skel, distancia);
                            this.prueba_coordenadas(skel);
                            this.DrawBonesAndJoints(skel, dc);
>>>>>>> origin/dev_kinect_inicial
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
       // DIBUJADO DEL ESQUELETO EN PANTALLA
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
           // PUNTOS DEL ESQUELETO QUE SE VAN A MOVER (en este caso la cadera)
           Joint caderaDerecha = skeleton.Joints[JointType.HipRight];
           Joint caderaIzquierda = skeleton.Joints[JointType.HipLeft];
           Joint caderaCentro = skeleton.Joints[JointType.HipCenter];

           // OBTENEMOS LAS POSICIONES DEL EJE X DE LOS PUNTOS DE LA CADERA
           double pos_cadDerecha = caderaDerecha.Position.Z;
           double pos_cadIzquierda = caderaIzquierda.Position.Z;
           double pos_cadCentro = caderaCentro.Position.Z;
           // VARIABLE BOOLEANA QUE INDICA LA CORRECCIÓN DEL MOVIMIENTO.

           // SI LA CADERA SE MUEVE HACIA ATRÁS, EL MOVIMIENTO ES CORRECTO
           if (pos_cadDerecha>0 && pos_cadIzquierda>0 && pos_cadCentro>0)
              correct = true;

            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter, correct);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft, correct);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight, correct);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine, correct);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter, correct);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft, correct);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight, correct);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft, correct);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft, correct);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft, correct);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight, correct);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight, correct);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight, correct);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft, correct);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft, correct);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft, correct);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight, correct);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight, correct);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight, correct);
 
            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;
<<<<<<< HEAD

                if (joint.TrackingState == JointTrackingState.Tracked && correct)
=======
               /*
                if (joint.TrackingState == JointTrackingState.Tracked)
>>>>>>> origin/dev_kinect_inicial
                {
                    drawBrush = this.trackedJointBrush;                    
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;                    
                }
               */

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                   if (estado != ESTADO_MOVIMIENTO.ERROR && estado != ESTADO_MOVIMIENTO.BEHIND && estado != ESTADO_MOVIMIENTO.COMPLETE)
                      drawBrush = this.hueso_movCorrecto;
                   else if (estado == ESTADO_MOVIMIENTO.ERROR)
                      drawBrush = this.hueso_error;
                   else if (estado == ESTADO_MOVIMIENTO.BEHIND)
                      drawBrush = this.hueso_distAlcanzada;
                   else if (estado == ESTADO_MOVIMIENTO.COMPLETE)
                      drawBrush = this.hueso_completado;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                   drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        // DIBUJAR LOS "HUESOS" ENTRE DOS PUNTOS jointType0 Y jointType1
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1, bool correct)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // SI AMBOS PUNTOS ESTÁN DETECTADOS Y EL MOVIMIENTO HA SIDO CORRECTO...
            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                //drawPen = this.trackedBonePen;
               if (estado != ESTADO_MOVIMIENTO.ERROR && estado != ESTADO_MOVIMIENTO.BEHIND && estado != ESTADO_MOVIMIENTO.COMPLETE)
                  drawPen = this.articulacion_movCorrecto;
               else if (estado == ESTADO_MOVIMIENTO.ERROR)
                  drawPen = this.articulacion_error;
               else if (estado == ESTADO_MOVIMIENTO.BEHIND)
                  drawPen = this.articulacion_distAlcanzada;
               else if (estado == ESTADO_MOVIMIENTO.COMPLETE)
                  drawPen = this.articulacion_completado;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        // COMPRUEBA SI EL ESQUELETO DETECTADO ESTÁ SENTADO O EN PIE
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
           estado = ESTADO_MOVIMIENTO.QUIET;
        }
    }
}