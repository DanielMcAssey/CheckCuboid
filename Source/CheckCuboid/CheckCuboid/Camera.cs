using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CheckCuboid
{
    public class Camera
    {
        //Objects
        protected GraphicsDevice _obj_graphics;
        protected InputManager _obj_input;

        //General Settings
        protected Vector3 mPosition;
        protected Vector3 mTarget;
        protected Matrix mViewMatrix;
        protected Matrix mProjectionMatrix;

        //Camera Settings
        protected float mYaw;
        protected float mPitch;
        protected float mRoll;
        protected float mSpeed;
        protected float mRotSpeed;
        protected Vector3 mDir;
        protected Matrix mRot;

        //Chase/Orbit Camera
        protected Vector3 mTargetOffset;
        protected Vector3 mTargetPos;
        protected Vector3 mTargetWho;

        #region "Properties"
        public Matrix View
        {
            get { return this.mViewMatrix; }
        }

        public Matrix Projection
        {
            get { return this.mProjectionMatrix; }
        }

        public Vector3 Position
        {
            get { return this.mPosition; }
            set { this.mPosition = value; }
        }

        public float Yaw
        {
            get { return this.mYaw; }
            set { this.mYaw = value; }
        }

        public Vector3 Direction
        {
            get
            {
                Vector3.Subtract(ref this.mTarget, ref this.mPosition, out mDir);
                return mDir;
            }
        }

        public float Pitch
        {
            get { return this.mPitch; }
            set { this.mPitch = value; }
        }

        public float Roll
        {
            get { return this.mRoll; }
            set { this.mRoll = value; }
        }

        public Matrix Rotation
        {
            get { return this.mRot; }
            set { this.mRot = value; }
        }

        public Vector3 Offset
        {
            get { return this.mTargetOffset; }
            set { this.mTargetOffset = value; }
        }
        #endregion

        public Camera(ref GraphicsDevice _graphics, ref InputManager _input)
        {
            this._obj_graphics = _graphics;
            this._obj_input = _input;
            this.ResetCamera();
        }

        public void SetPosition(Vector3 _newPosition)
        {
            this.mPosition = _newPosition;
        }

        public void ResetCamera()
        {
            this.mPosition = new Vector3(0, 0, 250);
            this.mTarget = Vector3.Zero;
            this.mViewMatrix = Matrix.Identity;
            this.mProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), this._obj_graphics.Viewport.AspectRatio, .1f, 10000.0f);

            this.mYaw = 0.0f;
            this.mPitch = 0.0f;
            this.mRoll = 0.0f;
            this.mRotSpeed = 1f;
            this.mRot = Matrix.Identity;

            //Custom
            this.mTargetPos = this.mPosition;
            this.mTargetWho = this.mTarget;
            this.mTargetOffset = new Vector3(0, 0, 250);
        }

        public void MoveCamera(Vector3 _moveVector)
        {
            this.mPosition += this.mSpeed * _moveVector;
        }

        public void RotateCamera(Vector3 _rotateVector)
        {
            this.mYaw += _rotateVector.Y;
            this.mPitch -= _rotateVector.X;
            this.mPitch = MathHelper.Clamp(this.mPitch, MathHelper.ToRadians(-5000), MathHelper.ToRadians(5000));
        }

        public void HandleInput(PlayerIndex? _controllingPlayer)
        {
            if (this._obj_input.IsPressed("CAMERA_RESET", _controllingPlayer))
                this.ResetCamera();

            if (this._obj_input.IsPressed("CAMERA_UP", _controllingPlayer))
                this.RotateCamera(Vector3.UnitX * 1.0f);

            if (this._obj_input.IsPressed("CAMERA_DOWN", _controllingPlayer))
                this.RotateCamera(Vector3.UnitX * -1.0f);

            if (this._obj_input.IsPressed("CAMERA_LEFT", _controllingPlayer))
                this.RotateCamera(Vector3.UnitY * -1.0f);

            if (this._obj_input.IsPressed("CAMERA_RIGHT", _controllingPlayer))
                this.RotateCamera(Vector3.UnitY * 1.0f);

            if (this._obj_input.IsPressed("CAMERA_ZOOM_IN", _controllingPlayer))
                this.mTargetOffset += new Vector3(0f, 0f, 5f);

            if (this._obj_input.IsPressed("CAMERA_ZOOM_OUT", _controllingPlayer))
                this.mTargetOffset += new Vector3(0f, 0f, -5f);
        }

        public void Update(float timeDiff, Matrix _chaseWorld)
        {
            this.mRot.Forward.Normalize();

            this.mRot = Matrix.CreateRotationX(this.mPitch * timeDiff) * Matrix.CreateRotationY(this.mYaw * timeDiff) * Matrix.CreateFromAxisAngle(this.mRot.Forward, this.mRoll);
            this.mTargetPos = Vector3.Transform(this.mTargetOffset, this.mRot);
            this.mTargetPos += _chaseWorld.Translation;
            this.mPosition = this.mTargetPos;
            this.mTarget = _chaseWorld.Translation;
            this.mRoll = MathHelper.SmoothStep(this.mRoll, 0f, .2f);
            this.mViewMatrix = Matrix.CreateLookAt(this.mPosition, this.mTarget, this.mRot.Up);
        }
    }
}
