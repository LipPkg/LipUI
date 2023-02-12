﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LipNETWrapper
{
    public class LipCommand
    {
        private readonly List<string> _commands = new();
        public static LipCommand Create()
        {
            return new LipCommand();
        }
        public static LipCommand Create(string cmd)
        {
            return new LipCommand().Add(cmd);
        }
        public LipCommand Add(string cmd)
        {
            _commands.Add(cmd);
            return this;
        }
        public LipCommand WithJson()
        {
            _commands.Add("--json");
            return this;
        }
        public LipCommand Verbose()
        {
            _commands.Add("--verbose");
            return this;
        }
        public LipCommand Quiet()
        {
            _commands.Add("--quiet");
            return this;
        }
        public static implicit operator string(LipCommand cmd) => string.Join(" ", cmd._commands);
        public override string ToString() => this;
        public static LipCommand operator +(LipCommand cmd, string s) => cmd.Add(s);
    }
    public class LipConsoleCommandInstance
    {
        private Process? _process;
        public bool HasExited => _process.HasExited;
        public int ExitCode => _process.ExitCode;
        private CancellationToken _tk;
        public LipConsoleCommandInstance(string exe, string cmd, CancellationToken tk, Action<string> output, Action<string> outputErr)
        {
            _tk = tk;
            _process = new();
            _process.StartInfo = new(exe, cmd)
            {
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WorkingDirectory = Path.GetDirectoryName(exe)
            };
            _process.OutputDataReceived += (_, args) =>
            {
                if (!_tk.IsCancellationRequested)
                    output(args.Data);
            };
            _process.ErrorDataReceived += (_, args) =>
            {
                if (!_tk.IsCancellationRequested) 
                    outputErr(args.Data);
            };
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }
        public void KillIfCanceled()
        {
            if (_tk.IsCancellationRequested)
            {
                try
                {
                    _process?.Kill();
                    _process = null;
                }
                catch
                {
                    // ignored
                }
                _tk.ThrowIfCancellationRequested();
            }
        }
        ~LipConsoleCommandInstance()
        {
            try
            {
                _process?.Kill();
            }
            catch
            {
                // ignored
            }
            _process?.Dispose();
        }
    }
    public class LipConsoleLoader
    {
        public LipConsoleLoader(string executablePath = "lip.exe")
        {
            ExecutablePath = Path.GetFullPath(executablePath);
        }
        public string ExecutablePath { get; }
        public async Task<int> Run(string cmd, Action<string>? output, CancellationToken tk = default)
        {
            var inst = new LipConsoleCommandInstance(ExecutablePath, cmd, tk, s => output?.Invoke(s), s => output?.Invoke(s));
            while (!inst.HasExited)
            {
                await Task.Delay(100, tk);
            }
            tk.ThrowIfCancellationRequested();
            return inst.ExitCode;
        }
        public async Task<int> Run(string cmd, Action<string> output, Action<string> outputError, CancellationToken tk = default)
        {
            var inst = new LipConsoleCommandInstance(ExecutablePath, cmd, tk, output, outputError);
            while (!inst.HasExited)
            {
                await Task.Delay(100, tk);
            }
            tk.ThrowIfCancellationRequested();
            return inst.ExitCode;
        }
        public async Task<string> RunString(string cmd, Action<string>? output = null, CancellationToken tk = default)
        {
            var sb = new StringBuilder();
            var inst = new LipConsoleCommandInstance(ExecutablePath, cmd, tk,
                s =>
                {
                    sb.AppendLine(s); output?.Invoke(s);
                }, s =>
                {
                    sb.AppendLine(s); output?.Invoke(s);
                });
            while (!inst.HasExited)
            {
                try
                {
                    await Task.Delay(100, tk);
                }
                catch
                {
                    // ignored
                }
                inst.KillIfCanceled();
            }
            tk.ThrowIfCancellationRequested();
            return sb.ToString();
        }
    }
}
