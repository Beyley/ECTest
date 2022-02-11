const std = @import("std");
const io = std.io;
const debug = std.debug;
const json = std.json;
const fs = std.fs;

const zigimg = @import("zigimg");
const Image = zigimg.Image;

pub fn main() anyerror!void {
    //Initialize our allocator
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    const allocator = gpa.allocator();
    //Check if we leaked any memory or not
    defer {
        const leaked = gpa.deinit();
        if (leaked) @panic("We leaked, fuck!\n");
    }

    var parser = std.json.Parser.init(allocator, false);
    defer parser.deinit();

    //Open the config file
    const file: std.fs.File = try std.fs.cwd().openFile(
        "zbuild.json",
        .{ .read = true },
    );
    defer file.close();

    var file_slice: []u8 = try file.readToEndAlloc(allocator, 1024);
    defer allocator.free(file_slice);

    var tree = try parser.parse(file_slice);
    defer tree.deinit();

    var image_folder = tree.root.Object.get("image_folder").?.String;

    debug.print("Converting png files in {s} to Qoi!\n", .{ image_folder });

    fs.cwd().makeDir(image_folder) catch {};

    const dir = try fs.cwd().openDir(image_folder, .{ .iterate = true });

    var file_count: usize = 0;
    var iter: fs.Dir.Iterator = dir.iterate();
    while (try iter.next()) |entry| {
        if(entry.kind == .File and std.ascii.endsWithIgnoreCase(entry.name, ".png")) {
            file_count += 1;
        
            debug.print("Found png file {s}!\n", .{entry.name});

            const path = try std.fmt.allocPrint(
                allocator,
                "{s}/{s}",
                .{ image_folder, entry.name }
            );
            defer allocator.free(path);

            const path_to_write = try std.fmt.allocPrint(
                allocator,
                "{s}/{s}.qoi",
                .{ image_folder, entry.name[0..entry.name.len - 4] }
            );
            defer allocator.free(path_to_write);

            var image: Image = try Image.fromFilePath(allocator, path);
            defer image.deinit();

            try image.writeToFilePath(path_to_write, zigimg.ImageFormat.Qoi, zigimg.image.ImageEncoderOptions.None);
        }
    }
    debug.print("Converted {d} images!\n", .{file_count});
}
