﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SlimDX;

using SB3Utility;

namespace ODFPlugin
{
	public partial class odfEditor : IDisposable
	{
		[Plugin]
		public void ReplaceMorph(WorkspaceMorph morph, string destMorphName, string newName, bool replaceNormals, double minSquaredDistance)
		{
			odf.ReplaceMorph(destMorphName, Parser, morph, newName, replaceNormals, (float)minSquaredDistance);
		}

		[Plugin]
		public void ReplaceAnimation(WorkspaceAnimation animation, int resampleCount, bool linear, string method, string clip, int insertPos, bool negateQuaternionFlips)
		{
			var replaceMethod = (ReplaceAnimationMethod)Enum.Parse(typeof(ReplaceAnimationMethod), method);
			ReplaceAnimation(animation, Parser, resampleCount, linear, replaceMethod, clip, insertPos, negateQuaternionFlips);
		}

		public static void ReplaceAnimation(WorkspaceAnimation wsAnimation, odfParser parser, int resampleCount, bool linear, ReplaceAnimationMethod replaceMethod, string clip, int insertPos, bool negateQuaternionFlips)
		{
			if (parser.AnimSection == null)
			{
				Report.ReportLog(Path.GetFileName(parser.ODFPath) + " doesn't have an animation section. Skipping this animation");
				return;
			}

			Report.ReportLog("Replacing animation ...");
			List<KeyValuePair<string, ImportedAnimationKeyframe[]>> newTrackList = FbxUtility.CopyAnimation(wsAnimation, resampleCount, linear);

			List<odfTrack> animationNodeList = odf.FindClip(clip, parser).ChildList;
			ImportedAnimation iAnim = new ImportedAnimation();
			iAnim.TrackList = new List<ImportedAnimationTrack>(animationNodeList.Count);
			Dictionary<string, ImportedAnimationTrack> animationNodeDic = null;
			if (replaceMethod != ReplaceAnimationMethod.Replace)
			{
				animationNodeDic = new Dictionary<string, ImportedAnimationTrack>();
				foreach (odfTrack animationNode in animationNodeList)
				{
					ImportedAnimationTrack iTrack = new ImportedAnimationTrack();
					iTrack.Name = odf.FindFrame(animationNode.BoneFrameId, parser.FrameSection.RootFrame).Name;
					iTrack.Keyframes = Plugins.ODFConverter.ConvertTrack(animationNode.KeyframeList);
					animationNodeDic.Add(odf.FindFrame(animationNode.BoneFrameId, parser.FrameSection.RootFrame).Name, iTrack);
					iAnim.TrackList.Add(iTrack);
				}
			}

			FbxUtility.ReplaceAnimation(replaceMethod, insertPos, newTrackList, iAnim, animationNodeDic, negateQuaternionFlips);

			animationNodeList.Clear();
			foreach (var newTrack in iAnim.TrackList)
			{
				odfTrack animationNode = new odfTrack(newTrack.Keyframes.Length);
				odf.CreateUnknowns(animationNode);
				animationNodeList.Add(animationNode);
				animationNode.KeyframeList = Plugins.ODFConverter.ConvertTrack(newTrack.Keyframes);
				animationNode.BoneFrameId = odf.FindFrame(newTrack.Name, parser.FrameSection.RootFrame).Id;
			}
		}

		[Plugin]
		public void SetAnimationClipName(string name, string newName)
		{
			odfANIMSection anim = odf.FindClip(name, Parser);
			anim.Name = new ObjectName(newName, null);
		}

		[Plugin]
		public void SetAnimationClipStart(string name, int start)
		{
			odfBANMSection anim = odf.FindClip(name, Parser) as odfBANMSection;
			if (anim != null)
			{
				anim.StartKeyframeIndex = start;
			}
		}

		[Plugin]
		public void SetAnimationClipEnd(string name, int end)
		{
			odfBANMSection anim = odf.FindClip(name, Parser) as odfBANMSection;
			if (anim != null)
			{
				anim.EndKeyframeIndex = end;
			}
		}
	}
}