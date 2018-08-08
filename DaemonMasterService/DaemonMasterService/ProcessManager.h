//  DaemonMasterService: ProcessManager
//  
//  This file is part of DeamonMasterService.
// 
//  DeamonMaster is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//   DeamonMaster is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with DeamonMasterService.  If not, see <http://www.gnu.org/licenses/>.
/////////////////////////////////////////////////////////////////////////////////////////

#pragma once
#include "RegistryManager.h"

class ProcessManager
{
public:
	ProcessManager();
	~ProcessManager();

	void SetStartMode(bool startInUserSession);
	bool StartProcess(const std::wstring& serviceName);
	bool StopProcess();
	void KillProcess();

private:
	void StartInSession0();

	RegistryManager::ProcessStartInfo _processStartInfo;
	PROCESS_INFORMATION _pi;
	bool _startInUserSession{ false };
};
