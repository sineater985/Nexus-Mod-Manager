﻿using System;
using System.Collections.Generic;
using Nexus.Client.ModRepositories;
using Nexus.Client.Mods;

namespace Nexus.Client.ModManagement
{
	/// <summary>
	/// Tags mods with metadata retrieved from a mod repository.
	/// </summary>
	public class AutoTagger
	{
		#region Properties

		/// <summary>
		/// Gets the mod repository from which to get mod info.
		/// </summary>
		/// <value>The mod repository from which to get mod info.</value>
		protected IModRepository ModRepository { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object wiht the given values.
		/// </summary>
		/// <param name="p_mrpModRepository">The mod repository from which to get mods and mod metadata.</param>
		public AutoTagger(IModRepository p_mrpModRepository)
		{
			ModRepository = p_mrpModRepository;
		}

		#endregion

		/// <summary>
		/// Gets a list of possible mod info tags which match the given mod.
		/// </summary>
		/// <param name="p_modMod">The mod for which to retrieve a list of possible tags.</param>
		/// <returns>A list of possible mod info tags which match the given mod.</returns>
		public IEnumerable<IModInfo> GetTagInfoCandidates(IMod p_modMod)
		{
			//get mod info
			List<IModInfo> lstMods = new List<IModInfo>();
			IModInfo mifInfo = null;
			if (!String.IsNullOrEmpty(p_modMod.Id))
				mifInfo = ModRepository.GetModInfo(p_modMod.Id);
			if (mifInfo == null)
				mifInfo = ModRepository.GetModInfoForFile(p_modMod.Filename);
			if (mifInfo == null)
			{
				//use heuristics to find info
				lstMods.AddRange(ModRepository.FindMods(p_modMod.ModName, true));
				if (lstMods.Count == 0)
					lstMods.AddRange(ModRepository.FindMods(p_modMod.ModName, false));
			}
			else
				lstMods.Add(mifInfo);

			//if we don't know the mod Id, then we have no way of getting
			// the file-specific info, so only look if we have one mod info
			// candidate.
			if (lstMods.Count == 1)
			{
				mifInfo = lstMods[0];
				lstMods.Clear();
				//get file specific info
				IModFileInfo mfiFileInfo = ModRepository.GetFileInfoForFile(p_modMod.Filename);
				if (mfiFileInfo == null)
				{
					foreach (IModFileInfo mfiModFileInfo in ModRepository.GetModFileInfo(mifInfo.Id))
						lstMods.Add(CombineInfo(mifInfo, mfiModFileInfo));
				}
				else
					lstMods.Add(CombineInfo(mifInfo, mfiFileInfo));
				if (lstMods.Count == 0)
					lstMods.Add(mifInfo);
			}
			return lstMods;
		}

		/// <summary>
		/// Combines the given mod info and mod file info into one mod info.
		/// </summary>
		/// <param name="p_mifInfo">The mod info to combine.</param>
		/// <param name="p_mfiFileInfo">The mod file info to combine.</param>
		/// <returns>A mid info representing the information from both given info objects.</returns>
		public static IModInfo CombineInfo(IModInfo p_mifInfo, IModFileInfo p_mfiFileInfo)
		{
			ModInfo mifUpdatedInfo = new ModInfo(p_mifInfo);
			if (!String.IsNullOrEmpty(p_mfiFileInfo.HumanReadableVersion))
			{
				mifUpdatedInfo.HumanReadableVersion = p_mfiFileInfo.HumanReadableVersion;
				mifUpdatedInfo.MachineVersion = null;
			}
			if (!String.IsNullOrEmpty(p_mfiFileInfo.Name))
				mifUpdatedInfo.ModName = String.Format("{0} - {1}", p_mifInfo.ModName, p_mfiFileInfo.Name);
			return mifUpdatedInfo;
		}

		/// <summary>
		/// Tags the mod with the given values.
		/// </summary>
		/// <param name="p_modMod">The mod the tag.</param>
		/// <param name="p_mifModInfo">The values with which to tag the mod.</param>
		/// <param name="p_booOverwriteAllValues">Whether to overwrite the current info values,
		/// or just the empty ones.</param>
		public void Tag(IMod p_modMod, IModInfo p_mifModInfo, bool p_booOverwriteAllValues)
		{
			p_modMod.UpdateInfo(p_mifModInfo, p_booOverwriteAllValues);
		}
	}
}