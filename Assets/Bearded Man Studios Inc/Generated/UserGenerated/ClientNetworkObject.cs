using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0]")]
	public partial class ClientNetworkObject : NetworkObject
	{
		public const int IDENTITY = 2;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		private Color _color;
		public event FieldEvent<Color> colorChanged;
		public Interpolated<Color> colorInterpolation = new Interpolated<Color>() { LerpT = 0f, Enabled = false };
		public Color color
		{
			get { return _color; }
			set
			{
				// Don't do anything if the value is the same
				if (_color == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_color = value;
				hasDirtyFields = true;
			}
		}

		public void SetcolorDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_color(ulong timestep)
		{
			if (colorChanged != null) colorChanged(_color, timestep);
			if (fieldAltered != null) fieldAltered("color", _color, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			colorInterpolation.current = colorInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _color);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_color = UnityObjectMapper.Instance.Map<Color>(payload);
			colorInterpolation.current = _color;
			colorInterpolation.target = _color;
			RunChange_color(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _color);

			// Reset all the dirty fields
			for (int i = 0; i < _dirtyFields.Length; i++)
				_dirtyFields[i] = 0;

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);

			if ((0x1 & readDirtyFlags[0]) != 0)
			{
				if (colorInterpolation.Enabled)
				{
					colorInterpolation.target = UnityObjectMapper.Instance.Map<Color>(data);
					colorInterpolation.Timestep = timestep;
				}
				else
				{
					_color = UnityObjectMapper.Instance.Map<Color>(data);
					RunChange_color(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (colorInterpolation.Enabled && !colorInterpolation.current.UnityNear(colorInterpolation.target, 0.0015f))
			{
				_color = (Color)colorInterpolation.Interpolate();
				//RunChange_color(colorInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public ClientNetworkObject() : base() { Initialize(); }
		public ClientNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public ClientNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
