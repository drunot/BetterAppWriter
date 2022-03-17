#!/usr/bin/env python3
import subprocess
import os
import shutil
import sys


def main():
    # Default path
    PATH = r"C:\Program Files (x86)\Wizkids\AppWriter\Lib\nlp.dll"
    if len(sys.argv) > 1:
        PATH = sys.argv[1]
    folder_name = os.path.basename(PATH).split(".")[0]
    wd = os.getcwd()
    os.chdir("./Vendor/wrap_dll")
    subprocess.run(["python", "wrap_dll.py", PATH])
    os.chdir(wd)
    if os.path.exists("nlp_loader"):
        if os.path.isdir("nlp_loader"):
            shutil.move(
                f"./Vendor/wrap_dll/{folder_name}/nlp.cpp", "./nlp_loader/nlp.cpp"
            )
            shutil.move(
                f"./Vendor/wrap_dll/{folder_name}/nlp.def", "./nlp_loader/nlp.def"
            )
            shutil.move(
                f"./Vendor/wrap_dll/{folder_name}/real_nlp.dll",
                "./nlp_loader/real_nlp.dll",
            )
            shutil.rmtree(f"./Vendor/wrap_dll/{folder_name}")
        else:
            print('"nlp_loader" must be a directory')
            shutil.rmtree(f"./Vendor/wrap_dll/{folder_name}")
    else:
        shutil.move(f"./Vendor/wrap_dll/{folder_name}", "./nlp_loader")


if __name__ == "__main__":
    main()
