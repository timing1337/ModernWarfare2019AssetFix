using static System.Net.Mime.MediaTypeNames;
using System;
using System.Linq;
using System.Text;

if (args.Length < 1)
{
    Console.WriteLine("Drag your folder here");
    return;
}

void processModel(string path)
{
    foreach (string file in Directory.EnumerateFiles(path, "*.txt"))
    {
        Console.WriteLine($"Procesing ${file}");

        string materialName = String.Join("_", Path.GetFileNameWithoutExtension(file).Split(".")[0].Split("_").SkipLast(1));
        Console.WriteLine($"Material: ${materialName}");

        string material_unparsed = File.ReadAllText(file);
        List<string[]> material_parsed = material_unparsed.Split("\n").Select(line => line.Split(",")).Skip(1).SkipLast(1).ToList();

        Dictionary<string, string> fixed_data = new Dictionary<string, string>();
        foreach (var shader_data in material_parsed)
        {
            string shader_id = shader_data[0];
            string texture_name = shader_data[1];
            if ((shader_id == "unk_semantic_0x9" || shader_id == "unk_semantic_0x4") && (texture_name.Contains("_n") && texture_name.Contains("&")))
            {
                string special_texture_name = String.Join("_", texture_name.Split("&")[0].Split("_").SkipLast(1));
                if(
                    Directory.GetFiles(Path.Join(path, "_images", materialName), special_texture_name + "_n.*").Count() > 0 ||
                    Directory.GetFiles(Path.Join(path, "_images"), special_texture_name + "_n.*").Count() > 0
                )
                {
                    fixed_data["normalMap"] = special_texture_name + "_n";
                    fixed_data["glossMap"] = special_texture_name + "_g";
                    fixed_data["aoMap"] = special_texture_name + "_o";
                }
                else
                {
                    fixed_data["normalMap"] = texture_name + "_n";
                    fixed_data["glossMap"] = texture_name + "_g";
                    fixed_data["aoMap"] = texture_name + "_o";
                }
            }
            else if (shader_id == "unk_semantic_0x12")
            {
                fixed_data["emissiveMap"] = texture_name;
            }
            else if (shader_id == "unk_semantic_0x1B" && texture_name.Contains("_a"))
            {
                fixed_data["alphaMap"] = texture_name;
            }
            else
            {
                fixed_data[shader_id] = texture_name;
            }
        }
        StringBuilder sb = new StringBuilder();
        sb.Append("semantic,image_name");
        foreach (var fixed_shader_data in fixed_data)
        {
            sb.Append("\n");
            sb.Append(fixed_shader_data.Key.ToString() + "," + fixed_shader_data.Value.ToString());
        }
        File.WriteAllText(file, sb.ToString());
    }
}

Console.WriteLine("Target: " + args[0]);
processModel(args[0]);
foreach(string path in Directory.GetDirectories(args[0]))
{
    processModel(path);
}
Thread.Sleep(10000);