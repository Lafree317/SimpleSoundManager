Simple Sound Manager
(c) 2017 LightGive


[�o�[�W����]
1.0.0


[�g�p���@]

�E�܂��A�g�p���鉹�t�@�C�����uAssets / SimpleSoundManager / Source / BGM or SE�v�̃t�H���_�ɓ���Ă��������B

�E���̌�A�uAudioName.cs�v�������Ő�������܂��B����́ASE��BGM�̍Đ����Ɉ����Ƃ��Ďg�������o���܂��B

�E�Đ����@
�Đ����鉹�̖��O�̈����́AString�^��Enum�^�̂ǂ���ł������ł��B
��1�FAudioName.SE_ExamplesE(string�^)
��2�FAudioNameSE.SE_ExamplesE(enum�^)

��ԃV���v����BGM�̍Đ����@
SimpleSoundManager.Instance.PlayBGM(AudioName.SE_ExampleSE); or SimpleSoundManager.Instance.PlayBGM(AudioNameSE.SE_ExamplesE);

��ԃV���v����SE�̍Đ����@
SimpleSoundManager.Instance.PlaySE2D(AudioName.SE_ExampleSE); or SimpleSoundManager.Instance.PlaySE2D(AudioNameSE.SE_ExamplesE)

BGM�̃N���X�t�F�[�h�Đ�
SimpleSoundManager.Instance.PlayCrossFadeBGM(AudioName.BGM_ExampleBGM)