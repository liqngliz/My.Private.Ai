using System;
using Autofac.Core;

namespace My.Ai.App.Lib.Models;

public record FileMeta(string Path, string Description);

public record FileMetas(List<FileMeta> Metas)
{
    public static implicit operator FileMetas(List<FileMeta> fileMetas) => new FileMetas(fileMetas);
    public static explicit operator List<FileMeta>(FileMetas fileMetas) => fileMetas.Metas;
}