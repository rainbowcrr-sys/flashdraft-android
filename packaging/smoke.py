import os
import subprocess
import time

workspace = os.environ.get("GITHUB_WORKSPACE", ".")
apk = os.path.join(workspace, "dist", "FlashDraft-Android.apk")


def run(command):
    result = subprocess.run(command, shell=True, capture_output=True, text=True)
    return result.returncode, result.stdout + result.stderr


def main():
    rc, out = run('adb install -r "' + apk + '"')
    print("install rc=" + str(rc) + " " + out.strip())
    if rc != 0:
        print("::error::FD_SMOKE=INSTALL_FAILED")
        return 1

    rc, out = run("adb shell am start -n com.flashdraft.app/.MainActivity")
    print("start rc=" + str(rc) + " " + out.strip())
    time.sleep(12)

    rc, out = run("adb shell ps -A")
    alive = "com.flashdraft.app" in out
    print("process listing: " + out.strip()[-400:])
    if alive:
        print("::warning::FD_SMOKE=APP_ALIVE")
        return 0

    print("::error::FD_SMOKE=APP_EXITED")
    rc, out = run("adb logcat -d -t 300")
    for line in out.splitlines():
        if any(k in line for k in ("flashdraft", "AndroidRuntime", "FATAL", "Avalonia", "MonoDroid", "IllegalState")):
            print("::error::" + line[:240])
    return 1


raise SystemExit(main())
