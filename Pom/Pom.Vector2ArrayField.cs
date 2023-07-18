﻿using DevInterface;
using RWCustom;
using UnityEngine;

namespace Pom;

public static partial class Pom
{
	/// <summary>
	/// A <see cref="ManagedField"/> that stores an array of <see cref="Vector2"/>s
	/// </summary>
	public class Vector2ArrayField : ManagedField
	{
		/// <summary>
		/// Chosen representation type
		/// </summary>
		public Vector2ArrayRepresentationType RepresentationType;
		/// <summary>
		/// Number of vector2 nodes
		/// </summary>
		public int NodeCount;
		public bool IncludeParent;

		/// <summary>
		/// Creates a <see cref="Vector2ArrayField"/> and assigns the proper values that are used in the handle
		/// </summary>
		/// <param name="key">The key of the field that should be used in <see cref="ManagedData.GetValue{T}"/></param>
		/// <param name="nodeCount">The number of <see cref="Vector2"/>s that the field stores</param>
		/// <param name="includeParent">Sets if the field should use the parent as the first node</param>
		/// <param name="representationType">The type of the representation that should be created.</param>
		/// <param name="nodes">Default nodes that are assigned. Node ordering is bottom nodes left to right, then top nodes right to left.</param>
		public Vector2ArrayField(
			string key,
			int nodeCount,
			bool includeParent,
			Vector2ArrayRepresentationType representationType = Vector2ArrayRepresentationType.Polygon,
			params Vector2[] nodes) : base(
				key,
				ProcessNodes(nodes, nodeCount))
		{
			NodeCount = nodeCount;
			RepresentationType = representationType;
			IncludeParent = includeParent;
		}
		/// <summary>
		/// Creates a <see cref="Vector2ArrayField"/> and assigns the proper values that are used in the handle
		/// </summary>
		/// <param name="key">The key of the field that should be used in <see cref="ManagedData.GetValue{T}"/></param>
		/// <param name="nodeCount">The number of <see cref="Vector2"/>s that the field stores</param>
		/// <param name="includeParent">Sets if the field should use the parent as the first node</param>
		/// <param name="representationType">The type of the representation that should be created.</param>
		/// <param name="nodeElements">Default nodes that are assigned. Node ordering is bottom nodes left to right, then top nodes right to left. Nodes are passed as pairs of floats: x0, y0, x1, y1 etc</param>
		public Vector2ArrayField(
			string key,
			int nodeCount,
			bool includeParent,
			Vector2ArrayRepresentationType representationType = Vector2ArrayRepresentationType.Polygon,
			params float[] nodeElements) : this(
				key,
				nodeCount,
				includeParent,
				representationType,
				MakeVectors(nodeElements))
		{

		}
		/// <inheritdoc/>
		public override string ToString(object value)
		{
			Vector2[] vectors = (Vector2[])value;
			return string.Join("^", vectors.Select(v => $"{v.x};{v.y}").ToArray());
		}
		/// <inheritdoc/>
		public override object FromString(string str)
		{
			List<Vector2> positions = new List<Vector2>();
			foreach (string substring in str.Split('^'))
			{
				string[] split = substring.Split(';');
				positions.Add(new Vector2(float.Parse(split[0]), float.Parse(split[1])));
			}

			return positions.ToArray();
		}

		private static object ProcessNodes(Vector2[] vector2s, int nodeCount)
		{
			if (vector2s is null) return new Vector2[nodeCount];
			Vector2[] result = new Vector2[nodeCount];
			Array.Copy(vector2s, result, vector2s.Length < nodeCount ? vector2s.Length : nodeCount);
			return result;
		}
		/// <inheritdoc/>
		public override DevUINode MakeAditionalNodes(ManagedData managedData, ManagedRepresentation managedRepresentation)
		{
			return new Vector2ArrayHandle(this, managedData, managedRepresentation);
		}
		/// <summary>
		/// How the handle should look like
		/// </summary>
		public enum Vector2ArrayRepresentationType
		{
			/// <summary>
			/// Handle is an open-ended chain
			/// </summary>
			Chain,
			/// <summary>
			/// Handle is a polygon (start and end connected)
			/// </summary>
			Polygon
		}
		/// <summary>
		/// Special handle for vector2arrayfield
		/// </summary>
		public class Vector2ArrayHandle : PositionedDevUINode
		{
			/// <summary>
			/// Associated field
			/// </summary>
			public Vector2ArrayField Field;
			/// <summary>
			/// Associated placedobject's data
			/// </summary>
			public ManagedData Data;
			/// <summary>
			/// First node
			/// </summary>
			public PositionedDevUINode First;
			/// <summary>
			/// List of line subnode indices
			/// </summary>
			public List<int> Lines = new List<int>();

			/// <param name="field">Field definition</param>
			/// <param name="data">Associated placedobject's data</param>
			/// <param name="representation">Associated placedobject's representation</param>
			public Vector2ArrayHandle(
				Vector2ArrayField field,
				ManagedData data,
				ManagedRepresentation representation) : base(
					representation.owner,
					field.key,
					representation,
					(data.GetValue<Vector2[]>(field.key) ?? new Vector2[] { default })[0])
			{
				Field = field;
				Data = data;
				bool includeParent = Field.IncludeParent;
				if (includeParent)
				{
					First = (parentNode as PositionedDevUINode)!;
				}
				else
				{
					First = new Handle(owner, field.key + "_0", this, Data.GetValue<Vector2[]>(field.key)![0]);
					subNodes.Add(First);
				}

				for (int i = 1; i < field.NodeCount; i++)
				{
					int currentLine = fSprites.Count;
					PositionedDevUINode next = new Handle(owner, field.key + "_" + i, this, Data.GetValue<Vector2[]>(field.key)![i]);
					subNodes.Add(next);
					fSprites.Add(new FSprite("pixel"));
					owner.placedObjectsContainer.AddChild(fSprites[currentLine]);
					fSprites[currentLine].anchorY = 0;
					Lines.Add(currentLine);
				}

				if (Field.RepresentationType == Vector2ArrayRepresentationType.Polygon)
				{
					int currLine = fSprites.Count;
					fSprites.Add(new FSprite("pixel"));
					owner.placedObjectsContainer.AddChild(fSprites[currLine]);
					fSprites[currLine].anchorY = 0;
					Lines.Add(currLine);
				}
			}
			/// <inheritdoc/>
			public override void Move(Vector2 newPos)
			{
				First.Move(newPos);
				Vector2[] vArr = Data.GetValue<Vector2[]>(Field.key)!;
				vArr[0] = Field.IncludeParent ? new Vector2(0, 0) : newPos;
				Data.SetValue(Field.key, vArr);
				base.Move(newPos);
			}
			/// <inheritdoc/>
			public override void Refresh()
			{
				base.Refresh();
				PositionedDevUINode start = First;
				int offset = Field.IncludeParent ? 0 : 1;
				for (int i = offset; i < subNodes.Count; i++)
				{
					if (!(subNodes[i] is PositionedDevUINode end))
					{
						throw new NullReferenceException("end node cannot be null!!");
					}

					int lineIndex = Lines[i - offset];
					MoveSprite(lineIndex, start.absPos);
					FSprite lineSprite = fSprites[lineIndex];
					lineSprite.scaleY = (end.absPos - start.absPos).magnitude;
					lineSprite.rotation = Custom.VecToDeg(end.absPos - start.absPos);
					start = end;
				}

				if (Field.RepresentationType == Vector2ArrayRepresentationType.Polygon)
				{
					int lastIndex = Lines[Lines.Count - 1];
					MoveSprite(lastIndex, start.absPos);
					FSprite lineSprite = fSprites[lastIndex];
					lineSprite.scaleY = (start.absPos - First.absPos).magnitude;
					lineSprite.rotation = Custom.VecToDeg(First.absPos - start.absPos);
				}
			}
		}

		private static Vector2[] MakeVectors(float[] floats)
		{
			Vector2[] res = new Vector2[floats.Length / 2];
			for (int i = 0; i < res.Length; i++)
			{
				res[i] = new(floats[i * 2], floats[i * 2 + 1]);
			}
			return res;
		}
	}
}
