﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Launcher {

  public interface ITrigger {

    bool didFire { get; }
    // bool isFiring { get; }
    // bool didRelease { get; }

  }

}
