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
        import base64

        def capture(name):
            png = os.path.join(workspace, name + ".png")
            # Software-rendered emulator surfaces keep stale tiles until one
            # full compose has run; discard that warm-up capture.
            subprocess.run("adb exec-out screencap -p > /dev/null", shell=True)
            time.sleep(0.8)
            with open(png, "wb") as handle:
                subprocess.run("adb exec-out screencap -p", shell=True, stdout=handle)
            blob = base64.b64encode(open(png, "rb").read()).decode()
            open(os.path.join(workspace, name + "-b64.txt"), "w").write(blob)
            print("FD_" + name.upper() + "_B64=" + str(len(blob)))

        capture("phone")

        # Best effort: accept the license and switch to the standalone About
        # page. This is evidence capture only; failures never fail the smoke.
        try:
            run("adb shell input tap 26 395")
            time.sleep(0.6)
            run("adb shell input tap 160 467")
            time.sleep(1.6)
            run("adb shell input tap 240 610")
            time.sleep(1.6)
            capture("about")
        except Exception as exc:
            print("about capture skipped: " + str(exc))
        return 0

    print("::error::FD_SMOKE=APP_EXITED")
    rc, out = run("adb logcat -d -t 300")
    for line in out.splitlines():
        if any(k in line for k in ("flashdraft", "AndroidRuntime", "FATAL", "Avalonia", "MonoDroid", "IllegalState")):
            print("::error::" + line[:240])
    return 1


raise SystemExit(main())
