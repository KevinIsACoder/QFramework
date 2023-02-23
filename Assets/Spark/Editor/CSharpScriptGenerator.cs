using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark;

namespace SparkEditor.UI
{
	public class CSharpScriptGenerator : ScriptGenerator
	{
		public CSharpScriptGenerator()
		{
			extension = "cs";
		}

		public override void Generate(string file, List<KeyValuePair<string, Type>> properties, List<UIViewExporter.UISubViewCollection> tableViews, List<UIViewExporter.UISubViewCollection> viewStacks, string prefabPath)
		{
			string className = Path.GetFileNameWithoutExtension(file);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("using System;");
			builder.AppendLine("using System.Collections;");
			builder.AppendLine("using System.Collections.Generic;");
			builder.AppendLine("using UnityEngine;");
			builder.AppendLine("using Spark;");
			builder.AppendLine("");
			builder.AppendLine("#pragma warning disable 0219 // variable assigned but not used.");
			builder.AppendLine("");

			builder.AppendFormat("public class {0} : Spark.UIView", className).AppendLine();
			builder.AppendLine("{");
			if (properties.Count > 0) {
				foreach (var kv in properties) {
					builder.Append("\t").AppendFormat("protected {0} {1};", kv.Value, "m_" + kv.Key).AppendLine();
				}
				builder.AppendLine("");
			}
			if (viewStacks != null && viewStacks.Count > 0) {
				foreach (var collection in viewStacks) {
					for (int i = 0; i < collection.views.Count; i++) {
						var view = collection.views[i];
						if (view.properties.Count > 0) {
							builder.Append("\t").AppendFormat("// ViewStack's Properties [{0} (index:{1}, name:{2})]", collection.name, i, view.name).AppendLine();
							foreach (var kv in view.properties) {
								builder.Append("\t").AppendFormat("protected {0} {1};", kv.Value, "m_" + kv.Key).AppendLine();
							}
							builder.AppendLine("");
						}
					}
				}
			}

			builder.Append("\t").AppendLine("protected override string prefabPath");
			builder.Append("\t").AppendLine("{");
			builder.Append("\t\t").AppendLine("get");
			builder.Append("\t\t").AppendLine("{");
			builder.Append("\t\t\t").AppendFormat("return \"{0}\";", prefabPath).AppendLine();
			builder.Append("\t\t").AppendLine("}");
			builder.Append("\t").AppendLine("}");
			builder.AppendLine();

			builder.Append("\t").AppendLine("sealed protected override void OnCreated()");
			builder.Append("\t").AppendLine("{");
			builder.Append("\t\t").AppendLine("base.OnCreated();");
			if (properties.Count > 0) {
				builder.Append("\t\t").AppendLine("var components = this.GetComponents(this.transform, true);");
				for (int i = 0; i < properties.Count; i++) {
					var kv = properties[i];
					builder.Append("\t\t").AppendFormat("this.m_{0} = components.Get<{1}>({2});", kv.Key, kv.Value, i).AppendLine();
				}
			}
			builder.Append("\t").AppendLine("}");
			if ((tableViews != null && tableViews.Count > 0) || (viewStacks != null && viewStacks.Count > 0)) {
				builder.AppendLine();
				builder.Append("\t").AppendLine("private Dictionary<Transform, object> mCachedViews = new Dictionary<Transform, object>();");
				builder.Append("\t").AppendLine("protected override void OnDestroyed()");
				builder.Append("\t").AppendLine("{");
				builder.Append("\t\t").AppendLine("mCachedViews.Clear();");
				builder.Append("\t\t").AppendLine("base.OnDestroyed();");
				builder.Append("\t").AppendLine("}");
			}

			if (viewStacks != null && viewStacks.Count > 0) {
				builder.AppendLine();
				builder.Append("\t").AppendLine("sealed protected override void OnBeforeViewStackValueChanged(UIViewStack viewStack)");
				builder.Append("\t").AppendLine("{");
				builder.Append("\t\t").AppendLine("base.OnBeforeViewStackValueChanged(viewStack);");
				builder.Append("\t\t").AppendLine("var transform = viewStack.selectedValue;");
				builder.Append("\t\t").AppendLine("if (transform == null || mCachedViews.ContainsKey(transform))");
				builder.Append("\t\t\t").AppendLine("return;");
				builder.AppendLine();
				builder.Append("\t\t").AppendLine("var index = viewStack.selectedIndex;");
				int m = 0;
				for (int i = 0; i < viewStacks.Count; i++) {
					var collection = viewStacks[i];
					if (collection.views.Any((view) => view.properties.Count > 0)) {
						if (m > 0) {
							builder.Append(" else ");
						} else {
							builder.Append("\t\t");
						}
						builder.AppendFormat("if (viewStack == m_{0}) {{", collection.name).AppendLine();
						builder.Append("\t\t\t").AppendLine("var components = this.GetComponents(transform, true);");
						if (collection.views.Count > 1) {
							int n = 0;
							for (int j = 0; j < collection.views.Count; j++) {
								var view = collection.views[j];
								if (view.properties.Count > 0) {
									if (n > 0) {
										builder.Append(" else ");
									} else {
										builder.Append("\t\t\t");
									}
									builder.AppendFormat("if (index == {0}) {{", j).AppendLine();
									for (int k = 0; k < view.properties.Count; k++) {
										var kv = view.properties[k];
										builder.Append("\t\t\t\t").AppendFormat("this.m_{0} = components.Get<{1}>({2});", kv.Key, kv.Value, k).AppendLine();
									}
									builder.Append("\t\t\t").Append("}");
									n++;
								}
							}
							if (n > 0) {
								builder.AppendLine();
							}
						} else {
							var view = collection.views[0];
							for (int j = 0; j < view.properties.Count; j++) {
								var kv = view.properties[j];
								builder.Append("\t\t\t").AppendFormat("this.m_{0} = components.Get<{1}>({2});", kv.Key, kv.Value, j).AppendLine();
							}
						}
						builder.Append("\t\t").Append("}");
						m++;
					}
				}
				if (m > 0) {
					builder.AppendLine();
				}
				builder.Append("\t\t").AppendLine("mCachedViews[transform] = index;");
				builder.Append("\t").AppendLine("}");
			}

			if (tableViews != null && tableViews.Count > 0) {
				builder.AppendLine();
				HashSet<string> generatedCells = new HashSet<string>();
				builder.Append("\t").AppendLine("public interface Cell { }");
				foreach (var collection in tableViews) {
					foreach(var cell in collection.views) {
						if (generatedCells.Contains(cell.identifier))
							continue;
						generatedCells.Add(cell.identifier);
						builder.Append("\t").AppendFormat("public class Cell_{0} : Cell", cell.identifier).AppendLine();
						builder.Append("\t").AppendLine("{");
						foreach (var kv in cell.properties) {
							builder.Append("\t\t").AppendFormat("public {0} {1};", kv.Value, kv.Key.ToLowerFirst()).AppendLine();
						}
						builder.Append("\t").AppendLine("}");
					}
				}
			}

			// if (tableViews != null && tableViews.Count > 0) {
			// 	builder.AppendLine();
			// 	foreach (var collection in tableViews) {
			// 		builder.Append("\t").AppendFormat("protected class TB{0}", collection.name).AppendLine();
			// 		builder.Append("\t").AppendLine("{");
			// 		if (collection.views.Count > 1) {
			// 			builder.Append("\t\t").AppendLine("public interface Cell { }");
			// 			int i = 0;
			// 			foreach (var cell in collection.views) {
			// 				builder.Append("\t\t").AppendFormat("public class Cell{0} : Cell", i++).AppendLine();
			// 				builder.Append("\t\t").AppendLine("{");
			// 				foreach (var kv in cell.properties) {
			// 					builder.Append("\t\t\t").AppendFormat("public {0} {1};", kv.Value, kv.Key.ToLowerFirst()).AppendLine();
			// 				}
			// 				builder.Append("\t\t").AppendLine("}");
			// 			}
			// 		} else {
			// 			builder.Append("\t\t").AppendLine("public class Cell");
			// 			builder.Append("\t\t").AppendLine("{");
			// 			foreach (var kv in collection.views[0].properties) {
			// 				builder.Append("\t\t\t").AppendFormat("public {0} {1};", kv.Value, kv.Key.ToLowerFirst()).AppendLine();
			// 			}
			// 			builder.Append("\t\t").AppendLine("}");
			// 		}
			// 		builder.AppendLine("");
			// 		builder.Append("\t\t").AppendFormat("public Cell Get(UITableViewCell tableCell, {0} owner)", className).AppendLine();
			// 		builder.Append("\t\t").AppendLine("{");
			// 		builder.Append("\t\t\t").AppendLine("object obj = null;");
			// 		builder.Append("\t\t\t").AppendLine("if (owner.mCachedViews.TryGetValue(tableCell.transform, out obj))");
			// 		builder.Append("\t\t\t\t").AppendLine("return (Cell)obj;");
			// 		builder.AppendLine("");
			// 		if (collection.views.Count > 1) {
			// 			builder.Append("\t\t\t").AppendLine("Cell cell = null;");
			// 			builder.Append("\t\t\t");
			// 			int i = 0;
			// 			foreach (var cell in collection.views) {
			// 				if (i > 0) {
			// 					builder.Append(" else ");
			// 				}
			// 				builder.AppendFormat("if (tableCell.identifier == \"{0}\") {{", cell.identifier).AppendLine();
			// 				builder.Append("\t\t\t\t").AppendFormat("var cell{0} = new Cell{0}();", i).AppendLine();
			// 				List<KeyValuePair<string, Type>> props = cell.properties;
			// 				if (props.Count > 0) {
			// 					builder.Append("\t\t\t\t").AppendLine("var components = owner.GetComponents(tableCell, false);");
			// 					int j = 0;
			// 					foreach (var p in props) {
			// 						builder.Append("\t\t\t\t").AppendFormat("cell{0}.{1} = components.Get<{2}>({3});", i, p.Key.ToLowerFirst(), p.Value, j++).AppendLine();
			// 					}
			// 				}
			// 				builder.Append("\t\t\t\t").AppendFormat("cell = cell{0};", i++).AppendLine();
			// 				builder.Append("\t\t\t").Append("}");
			// 			}
			// 			builder.AppendLine();
			// 		} else {
			// 			builder.Append("\t\t\t").AppendLine("Cell cell = new Cell();");
			// 			List<KeyValuePair<string, Type>> props = collection.views[0].properties;
			// 			if (props.Count > 0) {
			// 				builder.Append("\t\t\t").AppendLine("var components = owner.GetComponents(tableCell, false);");
			// 				int i = 0;
			// 				foreach (var p in props) {
			// 					builder.Append("\t\t\t").AppendFormat("cell.{0} = components.Get<{1}>({2});", p.Key.ToLowerFirst(), p.Value, i++).AppendLine();
			// 				}
			// 			}
			// 		}
			// 		builder.Append("\t\t\t").AppendLine("owner.mCachedViews[tableCell.transform] = cell;");
			// 		builder.Append("\t\t\t").AppendLine("return cell;");
			// 		builder.Append("\t\t").AppendLine("}");
			// 		builder.Append("\t").AppendLine("}");
			// 	}
			// }
			builder.AppendLine("}");

			FileHelper.WriteString(file, builder.ToString());
		}

		private void GenerateGetCellViewFunction(StringBuilder builder, List<UIViewExporter.UISubViewCollection> tableViews, Type typeName, string viewName, string cellName) {
			// TableView
			builder.Append("\t").AppendFormat("public Cell GetCellView({0} view, {1} cell)", viewName, cellName).AppendLine();
			builder.Append("\t").AppendLine("{");
			builder.Append("\t\t").AppendLine("object obj = null;");
			builder.Append("\t\t").AppendLine("if (mCachedViews.TryGetValue(cell.transform, out obj)) {");
			builder.Append("\t\t\t").AppendLine("return (Cell)obj;");
			builder.Append("\t\t").AppendLine("}");
			builder.Append("\t\t").AppendLine("Cell cellView = null;");

			int m = 0;
			builder.Append("\t\t");
			foreach(var collection in tableViews) {
				if (collection.typeName == typeName) {
					if (m++ > 0) {
						builder.Append(" else ");
					}
					if (string.IsNullOrEmpty(collection.name)) {
						builder.AppendFormat("if (view.name == \"{0}\") {{", collection.objectName).AppendLine();
					} else {
						builder.AppendFormat("if (view == m_{0}) {{", collection.name).AppendLine();
					}

					int n = 0;
					builder.Append("\t\t\t");
					foreach (var cell in collection.views) {
						if (n++ > 0) {
							builder.Append(" else ");
						}
						builder.AppendFormat("if (cell.identifier == \"{0}\") {{", cell.identifier).AppendLine();
						builder.Append("\t\t\t\t").AppendFormat("var cv = new Cell_{0}();", cell.identifier).AppendLine();
						List<KeyValuePair<string, Type>> props = cell.properties;
						if (props.Count > 0) {
							builder.Append("\t\t\t\t").AppendLine("var components = GetComponents(cell, false);");
							int j = 0;
							foreach (var p in props) {
								builder.Append("\t\t\t\t").AppendFormat("cv.{0} = components.Get<{1}>({2});", p.Key.ToLowerFirst(), p.Value, j++).AppendLine();
							}
						}
						builder.Append("\t\t\t\t").Append("cellView = cv;").AppendLine();
						builder.Append("\t\t\t").Append("}");
					}
					builder.AppendLine();
					builder.Append("\t\t").Append("}");
				}
			}
			builder.AppendLine();

			builder.Append("\t\t").AppendLine("mCachedViews[cell.transform] = cellView;");
			builder.Append("\t\t").AppendLine("return cellView;");
			builder.Append("\t").AppendLine("}");
		}
	}
}
