using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace RoboDodge
{
    public class FPSCamera : ICamerable
    {
        float leftrightRot;
        float updownRot;
        const float rotationSpeed = 0.0025f;
        Vector3 cameraPosition;
        MouseState originalMouseState;
        
        public float MoveSpeed { get; private set; }
        public float SpeedModifier { get; private set; }

        # region ICamerable 
        Matrix viewMatrix;
        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        }

        Matrix projectionMatrix;
        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
            set { projectionMatrix = value; }
        }

        Viewport viewPort;
        public Viewport ViewPort
        {
            get { return viewPort; }
            set { viewPort = value; }
        }

        public Vector3 Position
        {
            get { return cameraPosition; }
            set
            {
                cameraPosition = value;
                UpdateViewMatrix();
            }
        }

        private Quaternion _rotation = Quaternion.Identity;
        public Quaternion Rotation 
        {
            get { return _rotation; }
            set { _rotation = value; }
        }
        #endregion

        public BoundingBox BoundingBox; // box in which the camera is enclosed
        public BoundingBox[] BlockBoxes; // boxes the camera cannot go into

        public FPSCamera(Viewport viewPort, Vector3 startingPos, float lrRot, float udRot)
        {    
            this.leftrightRot = lrRot;
            this.updownRot = udRot;

            Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, leftrightRot)
                        * Quaternion.CreateFromAxisAngle(Vector3.Right, updownRot);

            this.cameraPosition = startingPos;
            this.viewPort = viewPort;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                                    MathHelper.ToRadians(GameConstants.ViewAngle), 
                                    viewPort.AspectRatio, 
                                    GameConstants.NearPlane, 
                                    GameConstants.FarPlane);
            
            UpdateViewMatrix();

            Mouse.SetPosition(viewPort.Width/2, viewPort.Height/2);
            originalMouseState = Mouse.GetState();

            MoveSpeed = 0.1f;
            SpeedModifier = 1.0f; // no modification
        }

        private void UpdateViewMatrix()
        {
            //Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
            Matrix cameraRotation = Matrix.CreateFromQuaternion(Rotation);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);
            Vector3 cameraFinalUpVector = cameraPosition + cameraRotatedUpVector;

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, Vector3.Up);
        }

        
        public void Update(MouseState currentMouseState, KeyboardState keyState, GamePadState gamePadState)
        {
            if (currentMouseState != originalMouseState)
            {                
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;

                leftrightRot -= rotationSpeed * xDifference;
                updownRot -= rotationSpeed * yDifference;


                float tempSpeed = Rotation.Y;
                tempSpeed -= (rotationSpeed * xDifference);

                Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, leftrightRot)
                            * Quaternion.CreateFromAxisAngle(Vector3.Right, updownRot);
                

                Mouse.SetPosition(viewPort.Width / 2, viewPort.Height / 2);
                UpdateViewMatrix();                
            }


            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W) 
                || currentMouseState.RightButton == ButtonState.Pressed)      //Forward
                AddToCameraPosition(new Vector3(0, 0, -1));
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))    //Backward
                AddToCameraPosition(new Vector3(0, 0, 1));
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))   //Right
                AddToCameraPosition(new Vector3(1, 0, 0));
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))    //Left
                AddToCameraPosition(new Vector3(-1, 0, 0));
            if (keyState.IsKeyDown(Keys.Q) || keyState.IsKeyDown(Keys.LeftControl))                                     //Up
                AddToCameraPosition(new Vector3(0, 1, 0));
            if (keyState.IsKeyDown(Keys.C))                                     //Down
                AddToCameraPosition(new Vector3(0, -1, 0));

            if (keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift))
            {
                SpeedModifier = 0.5f;
            }
            else if (keyState.IsKeyDown(Keys.Space))
            {
                SpeedModifier = 1.5f;
            }
            else
            {
                SpeedModifier = 1;
            }
        }

        private bool IsColliding(BoundingSphere sphere)
        {
            for (int i = 0; i < BlockBoxes.Length; i++)
            {
                if (BlockBoxes[i].Contains(sphere) != ContainmentType.Disjoint)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateFromQuaternion(Rotation);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);

            float moveSpeed = MoveSpeed * SpeedModifier;

            Vector3 tempPosition = cameraPosition + moveSpeed * rotatedVector;

            // make sure we're withing the bounding box
            if (GameConfig.Instance.FreeCamera || 
                BoundingBox.Contains(tempPosition) == ContainmentType.Contains && !IsColliding(new BoundingSphere(tempPosition,GameConstants.PlayerSphereRadius)))
            {
                cameraPosition = tempPosition;
            }

            UpdateViewMatrix();
        }


        //public float UpDownRot
        //{
        //    get { return updownRot; }
        //    set { updownRot = value; }
        //}

        //public float LeftRightRot
        //{
        //    get { return leftrightRot; }
        //    set { leftrightRot = value; }
        //}


        //public Vector3 TargetPosition
        //{
        //    get 
        //    {
        //        //Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
        //        Matrix cameraRotation = Matrix.CreateFromQuaternion(Rotation);


        //        Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);


        //        Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
        //        Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;
        //        return cameraFinalTarget;
        //    }
        //}
        
        public Vector3 Forward
        {
            get
            {
                //Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
                Matrix cameraRotation = Matrix.CreateFromQuaternion(Rotation);

                Vector3 cameraForward = new Vector3(0, 0, -1);
                Vector3 cameraRotatedForward = Vector3.Transform(cameraForward, cameraRotation);
                return cameraRotatedForward;
            }
        }

        //public Vector3 SideVector
        //{
        //    get
        //    {
        //        //Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
        //        Matrix cameraRotation = Matrix.CreateFromQuaternion(Rotation);

        //        Vector3 cameraOriginalSide = new Vector3(1, 0, 0);
        //        Vector3 cameraRotatedSide = Vector3.Transform(cameraOriginalSide, cameraRotation);
        //        return cameraRotatedSide;
        //    }
        //}
        //public Vector3 UpVector
        //{
        //    get
        //    {
        //        //Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
        //        Matrix cameraRotation = Matrix.CreateFromQuaternion(Rotation);

        //        Vector3 cameraOriginalUp = new Vector3(0, 1, 0);
        //        Vector3 cameraRotatedUp = Vector3.Transform(cameraOriginalUp, cameraRotation);
        //        return cameraRotatedUp;
        //    }
        //}
    }
}
