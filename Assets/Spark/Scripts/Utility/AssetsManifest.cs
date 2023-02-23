using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Spark {
	[System.Serializable]
	public class AssetsManifest : ScriptableObject {
		[System.Serializable]
		public struct Asset {
			[SerializeField]
			public string path;
			[SerializeField]
			public int index;
		}

		[System.Serializable]
		public class Bundle {
			[SerializeField]
			public string name;
			[SerializeField]
			public Asset[] assets;
			[SerializeField]
			public string[] scenes;
			[SerializeField]
			public int[] depends;
			[SerializeField]
			public int dependents;
		}

		[SerializeField]
		public Bundle[] bundles;

		[SerializeField]
		public string[] directories;
	}

	public class AssetsManifestProxy {
		public List<string> directories;
		public List<AssetsManifest.Bundle> bundles;

		public AssetsManifest ToManifest() {
			var manifest = ScriptableObject.CreateInstance<Spark.AssetsManifest>();
			manifest.bundles = bundles.ToArray();
			manifest.directories = directories.ToArray();
			return manifest;
		}

		public void CopyFrom(AssetsManifestProxy other) {
			var dict = new Dictionary<string, int>();
			for (int i = 0; i < directories.Count; i++) {
				dict[directories[i]] = i + 1;
			}
			for (int i = 0; i < other.directories.Count; i++) {
				var dir = other.directories[i];
				if (!dict.ContainsKey(dir)) {
					directories.Add(dir);
					dict[dir] = directories.Count;
				}
			}

			int baseIndex = bundles.Count;
			bundles.AddRange(other.bundles);
			foreach (var bundle in other.bundles) {
				for (int i = 0; i < bundle.depends.Length; i++) {
					bundle.depends[i] += baseIndex;
				}
				for (int i = 0; i < bundle.assets.Length; i++) {
					var index = bundle.assets[i].index;
					if (index > 0) {
						bundle.assets[i].index = dict[other.directories[index - 1]];
					}
				}
			}
		}
	}
}